# Twimager
Download images from listed Twitter accounts, and keep them latest.

## Environment
- Windows 10
- .NET Framework 4.7.1

## Installation
1. Download ZIP file at [Releases](https://github.com/Siketyan/Twimager/releases) page.
2. Extract the file.
3. Launch `Twimager.exe`.

## Building
Before building, please write the code below with your Twitter API credentials.

##### Twimager/Resources/TwitterKeys.cs (This template can be found in TwitterKeys.Template.cs)
```cs
namespace Twimager.Resources
{
    public class TwitterKeys
    {
        public const string ConsumerKey = ""; // Your consumer key
        public const string ConsumerSecret = ""; // Your consumer secret
    }
}
```

## Open Source Software License
- [Newtonsoft.Json (Json.NET)](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)
  > The MIT License (MIT)
  > 
  > Copyright (c) 2007 James Newton-King
  > 
  > Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
  > 
  > The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
  > 
  > THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

- [CoreTweet](https://github.com/CoreTweet/CoreTweet)
  > The MIT License (MIT)
  > 
  > CoreTweet - A .NET Twitter Library supporting Twitter API 1.1  
  > Copyright (c) 2013-2018 CoreTweet Development Team
  > 
  > Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
  > 
  > The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
  > 
  > THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
