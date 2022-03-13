# BirdWatching

Twitter bot capable of posting a random image from a directory.
Intended to be executed with a CronJob.
The bot was originally developed for the purpose of posting images of my [tortoises.](https://twitter.com/OurTortoiseLife).

## Building

To build the project you will need the .NET 6.0 SDK.

To build the project:

```bash
dotnet build
```

To run the project:

```bash
dotnet run
```

## Usage

The application looks for the following environment variables to run properly:

- "CONSUMER_KEY" - Twitter API consumer key
- "CONSUMER_SECRET" - Twitter API consumer secret
- "ACCESS_TOKEN" - Twitter user access token
- "ACCESS_TOKEN_SECRET" - Twitter user access secret
- "IMAGE_DIRECTORY" - Image directory that the bot should pull a random image from

If the bot is missing "ACCESS_TOKEN" or "ACCESS_TOKEN_SECRET" it will try to open a browser window for the user to complete Twitter PIN based authentication.

You can run this application by building and running the exe, or by running the above 'run' command.
