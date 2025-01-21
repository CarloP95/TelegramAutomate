# TelegramAutomate

Your personal/home/family TelegramBot that can share confidential files or download things at your home for you

### Rationale

I was simply looking for a solution to avoid exposing my IP and my personal home network to the Internet and i thought that maybe Telegram could help me with a simple Long Polling strategy.

Of course you could use the Webhook method of the Telegram.Bot library but in this way you'll need a public URL.

This project was simply designed to run on an old x32 Architecture to be a Home Server to download torrents using a Torrent Client installed on it and configured to download torrents in a specific folder and to access remotely using a Telegram chat to the files downloaded.

### Features

- Only the Admin configured with FirstName, SecondName, ID in the appsettings file can access the functions /nas and /torrent.
- The admin needs to authenticate itself when using /nas or /torrent function. He needs to use the password stored in the appsettings file.
- Every hour the authentication will expire and the admin needs to restore the authentication for security reason.

### Future implementation

- Bypass 50MB limit of Telegram API using Drive to temporary upload files and create a download Link available for some configured number of hours.
- Give a temporary permit to download files to another Telegram User
- Use a secure way to send password to the bot to avoid storing the password in the chat.