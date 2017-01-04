# GuruFollower

Uses the [SEC 13F filings](https://www.sec.gov/answers/form13f.htm) to show the current portfolios for your chosen institutional investors, their position changes from the previous quarter and a cumulative portfolio consisting of all their positions.
Something akin to [DataRoma](http://www.dataroma.com/m/home.php) or [GuruFocus](http://www.gurufocus.com/).

It can be run either from the command line or deployed to Azure as a web application using Azure Functions, DocumentDB and Azure Queue as backend.

### Todo

1. Medium - Implement some form of identity so that each person connecting to the web site has its own list of gurus
2. Hard - Implement the concept of groups so that each user can create sub-lists of gurus according to whatever criteria they choose
3. Easy - Implement history for each position in the web app as presented in the console app so that you can see the history of buying and selling for a guru for a position

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

You need to install:

- [Visual Studio Tools for Azure Functions Preview](https://buildazure.com/2016/12/14/azure-functions-visual-studio-tools-preview/comment-page-1/)
- Vue.js: best through the [CLI](https://vuejs.org/v2/guide/installation.html#CLI)

### Installing

Installing and running the console application is trivial:

1. Clone the repository
2. Open the solution in the root directory in Visual Studio 2015
3. Set `ConsoleFollower` as your startup project
4. Debug/Start Debugging

Installing and running the web application to test it locally is not easy:

1. Fork the repository so that you have your own Github copy of it
2. Create a new [Azure Function Application](https://azure.microsoft.com/en-us/services/functions/)
3. Create a new DocumentDB in Azure and call it `guru-portfolios`
4. Create a new Azure Storage Queue in Azure in the storage account created automatically in step 2
7. Clone the repository
8. Copy `GuruBackend\appsettings.json.template` to `GuruBackend\appsettings.json` and modify it to point to the resources created in step 3. and 4.
9. *Exclude `GuruBackend\appsettings.json` from git* as you don`t want to share your account key with the world
10. Open the solution in the root directory in Visual Studio 2015
11. Set `GuruBackend` as your startup project
12. Debug/Start Debugging
13. Open cmd.exe in `GuruFrontend\src\` and run `npm run dev`
14. Your default browser opens up and you can start running the application

Deploying the whole thing to Azure is difficult:

15. Set up continous deployment pointing to your github repository as described [here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-continuous-deployment)
16. Add the variables in your `GuruBackend\appsettings.json` file to your Azure Function Application by clicking on `Function App Settings` and then `Configure App Settings`
17. Run all the functions to and look at the logs for success (to call the GetXXX ones you may want to use something like [Postman](https://www.getpostman.com/))

## Running the tests

Tests are run as usual from inside the Visual Studio UI.

## Authors

* **Luca Bolognese** - *Initial work* - [lucabol](https://github.com/lucabol)
* **Mike Rousos** - *Review* - [mjrousos](https://github.com/mjrousos)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

