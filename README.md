# HermanDaily

Twitter bot responsible for posting daily images of my tortoise - [Herman](https://twitter.com/HermanThePog).

The bot is actually pretty generic and could easily be used for posting random jpgs from any image directory for any Twitter account.

## Building

To build the project you will need .NET Core 3.1.

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
