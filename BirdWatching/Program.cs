using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        // Start our application stopwatch. We use this to measure program duration.
        applicationTimer = new Stopwatch();
        applicationTimer.Start();

        File.AppendAllText(LogFile, Environment.NewLine);

        // Find the starting location of new log messages.
        LogFileStart = File.ReadLines(LogFile).Count();

        File.AppendAllText(LogFile, DateTime.UtcNow.ToString());
        File.AppendAllText(LogFile, Environment.NewLine);

        // Verify we have a consumer key for making API requests.
        if (string.IsNullOrWhiteSpace(ConsumerKey))
        {
            File.AppendAllText(LogFile, "No API consumer key specified." + Environment.NewLine);
            return;
        }

        // Verify we have a consumer secret for making API requests.
        if (string.IsNullOrWhiteSpace(ConsumerSecret))
        {
            File.AppendAllText(LogFile, "No API consumer secret specified." + Environment.NewLine);
            return;
        }

        // Verify that we have an image directory to pull media from.
        if (string.IsNullOrWhiteSpace(ImageDirectory))
        {
            File.AppendAllText(LogFile, "No image directory specified." + Environment.NewLine);
            return;
        }

        // Create Twitter client.
        // If we are missing an access token or the access token secret, authenticate with PIN.
        if (string.IsNullOrWhiteSpace(AccessToken) || string.IsNullOrWhiteSpace(AccessTokenSecret))
        {
            File.AppendAllText(LogFile, "Invalid or missing access credentials. Authenticating with PIN." + Environment.NewLine);

            var userCredentials = await Authenticate();

            userClient = new TwitterClient(userCredentials);
        }
        else
        {
            userClient = new TwitterClient(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret);
        }

        // Fetch the authenticated user.
        var user = await userClient.Users.GetAuthenticatedUserAsync();

        // Verify that we successfully authenticated with Twitter.
        if (string.IsNullOrWhiteSpace(user.IdStr))
        {
            File.AppendAllText(LogFile, "Failed to authenticate." + Environment.NewLine);
            return;
        }

        File.AppendAllText(LogFile, "Authenticated as user: " + user.Name + " - " + user.IdStr + Environment.NewLine);

        // Select a random image.
        var selectedImage = SelectRandomImage();

        // Verify that we were able to select an image.
        if (string.IsNullOrWhiteSpace(selectedImage))
        {
            File.AppendAllText(LogFile, "Could not find any images in the specified directory." + Environment.NewLine);
            return;
        }
        else
        {
            File.AppendAllText(LogFile, "Selected image: " + selectedImage + Environment.NewLine);
        }

        // Publish a tweet with the selected image media.
        var tweet = await PublishTweetWithMedia(selectedImage);

        if (string.IsNullOrWhiteSpace(tweet.IdStr))
        {
            File.AppendAllText(LogFile, "Tweet did not publish successfully." + Environment.NewLine);
            return;
        }

        File.AppendAllText(LogFile, "Tweet published: " + tweet.IdStr + Environment.NewLine);
        File.AppendAllText(LogFile, "Application execution completed!" + Environment.NewLine);

        // Stop the application timer and report the application duration.
        applicationTimer.Stop();
        File.AppendAllText(LogFile, "Application duration: " + applicationTimer.ElapsedMilliseconds + Environment.NewLine);

        DumpLog();
    }

    /// <summary>
    /// Initiates a PIN based authentication process for the user.
    /// </summary>
    /// <returns>Twitter credentials for constructing a client.</returns>
    private static async Task<ITwitterCredentials> Authenticate()
    {
        // Create a client.
        var appClient = new TwitterClient(ConsumerKey, ConsumerSecret);

        // Start the authentication process.
        var authenticationRequest = await appClient.Auth.RequestAuthenticationUrlAsync();

        // Go to the URL so that Twitter authenticates the user and gives him a PIN code.
        Process.Start(new ProcessStartInfo(authenticationRequest.AuthorizationURL)
        {
            UseShellExecute = true
        });

        // Ask the user to enter the pin code given by Twitter.
        File.AppendAllText(LogFile, "Please enter the pin code." + Environment.NewLine);
        var pinCode = Console.ReadLine();

        // Get user credentials from Twitter.
        return await appClient.Auth.RequestCredentialsFromVerifierCodeAsync(pinCode, authenticationRequest);
    }

    /// <summary>
    /// Gets all files in the specified image directory and selects a random image.
    /// </summary>
    /// <returns>Image path string.</returns>
    private static string SelectRandomImage()
    {
        var rand = new Random();

        // Get a list of image paths from the provided image directory.
        var images = Directory.GetFiles(ImageDirectory, "*.jpg");

        // Verify we found any images.
        if (images == null || images.Length < 1)
        {
            return null;
        }

        // Pick a random image.
        return images[rand.Next(images.Length)];
    }

    /// <summary>
    /// Reads and uploads the image data for the specified imagePath parameter,
    /// then publishes the media with the last write time date as the tweet text
    /// if the media upload was successful.
    /// </summary>
    /// <param name="imagePath">Image path string.</param>
    /// <returns>Tweet as ITweet.</returns>
    private static async Task<ITweet> PublishTweetWithMedia(string imagePath)
    {
        // Get the image write time.
        var imageDate = File.GetLastWriteTime(imagePath);

        // Read in the image.
        var imageByteArray = File.ReadAllBytes(imagePath);

        // Upload the image to Twitter.
        var uploadedImage = await userClient.Upload.UploadTweetImageAsync(imageByteArray);

        // Verify that the image uploaded successfully.
        if (uploadedImage == null || uploadedImage.HasBeenUploaded == false || uploadedImage.IsReadyToBeUsed == false)
        {
            File.AppendAllText(LogFile, "Image did not upload successfully, aborting." + Environment.NewLine);
            return null;
        }

        // Publish a tweet with the image media attached.
        return await userClient.Tweets.PublishTweetAsync(new PublishTweetParameters(imageDate.ToString())
        {
            Medias = { uploadedImage }
        });
    }

    /// <summary>
    /// Dumps the log file to console.
    /// </summary>
    private static void DumpLog()
    {
        var logLines = File.ReadLines(LogFile).Skip(LogFileStart);

        using StreamReader reader = File.OpenText(LogFile);
        foreach (string line in logLines)
        {
            Console.WriteLine(line);
        }
    }

    private static TwitterClient userClient;
    private static Stopwatch applicationTimer;
    private static int LogFileStart;
    private static readonly string LogFile = @"log.txt";
    private static readonly string ConsumerKey = Environment.GetEnvironmentVariable("CONSUMER_KEY");
    private static readonly string ConsumerSecret = Environment.GetEnvironmentVariable("CONSUMER_SECRET");
    private static readonly string AccessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");
    private static readonly string AccessTokenSecret = Environment.GetEnvironmentVariable("ACCESS_TOKEN_SECRET");
    private static readonly string ImageDirectory = Environment.GetEnvironmentVariable("IMAGE_DIRECTORY");
}