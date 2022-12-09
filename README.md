# Clement Oniovosa
## Web Crawler

### Prerequisites
1. [Visual studio](https://visualstudio.microsoft.com/downloads/) or [Rider](https://www.jetbrains.com/rider/download)
2. [Dotnet 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
3. Language: C#

### Restore packages on terminal
From root directory run: `dotnet restore`

### Build solution on terminal
From root directory run: `dotnet build`

### Run on terminal 
From root directory run: `cd Crawler && dotnet run -- "http://monzo.com"`

### Test on terminal
From root directory run: `dotnet test -v q --nologo -l:"console;verbosity=detailed"`