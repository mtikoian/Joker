﻿# Installation instructions
### Prerequesities:
[.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)
[Visual Studio 2019 16.6 with ASP.NET and web development](https://visualstudio.microsoft.com/cs/thank-you-downloading-visual-studio/?sku=community)

### Git
`git clone https://github.com/tomasfabian/Joker.git`

`CD Joker`

Blazor WebAssembly client with SignalR pubsub from Sql server. OData server hosting

#Work in progress
- OData client does not support WASM platform. HttpClient is added with package Microsoft.OData.Extensions.Client.
- Redis ISubscriber does not support WASM platform. SignalR is used instead of it.
