# DocStoreAPI
A simple asp.net core document managment api, backed by SQL Server, AzureAd, ActiveDirectory, Azure storage and SMB shares.

At it's core DocStorAPI is a simple document management API designed to be easily intergratable with other projects that require an auditable service.

## To-Do
- [ ] Implement metadata filtering to ensure that when values which consist of the FileName are changed then the relevant filename is changed aswell.
- [ ] Implement a Security Controller(s) to Manage Groups and Access Control Entity
- [ ] Add a Client library
- [ ] Add an example WPF app

## Version 2 improvements
- Implement simple polices based upon BuisnessArea to define the `stor` used for specific documents.
- Add Support for MySql as an alternative database
- Add Support for Comsmos DB as an alternative database (requires .net core 3)
- Add Support for AWS storage
- Look into supporting other Identity providers



# MIT License

Copyright (c) 2019 Liam Townsend

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.