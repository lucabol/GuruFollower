# GuruFollower

Uses the [SEC 13F filings](https://www.sec.gov/answers/form13f.htm) to show the current portfolios for your chosen institutional investors, their position changes from the previous quarter and a cumulative portfolio consisting of all their positions.
Something akin to [DataRoma](http://www.dataroma.com/m/home.php) or [GuruFocus](http://www.gurufocus.com/).

It can be run either from the command line or deployed to Azure as a web application using Azure Functions, DocumentDB and Azure Queue as backend.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

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
3. Create a new DocumentDB in Azure and call it 'guru-portfolios'
4. Create a new Azure Storage Queue in Azure in the storage account created automatically in step 2
7. Clone the repository
8. Copy 'GuruBackend\appsettings.json.template' to 'GuruBackend\appsettings.json' and modify it to point to the resources created in step 3. and 4.
9. *Exclude 'GuruBackend\appsettings.json' from git* as you don't want to share your account key with the world
10. Open the solution in the root directory in Visual Studio 2015
11. Set `GuruBackend` as your startup project
12. Debug/Start Debugging
13. Open cmd.exe in 'GuruFrontend\' and run 'npm run dev'
14. Your default browser opens up and you can start running the application

Deploying the whole thing to Azure is difficult:
15. Set up continous deployment pointing to your github repository as described [here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-continuous-deployment)
16. Add the variables in your 'GuruBackend\appsettings.json' file to your Azure Function Application by clicking on 'Function App Settings' and then 'Configure App Settings'
17. Run all the functions to and look at the logs for success (to call the GetXXX ones you may want to use something like [Postman](https://www.getpostman.com/))


```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* [Dropwizard](http://www.dropwizard.io/1.0.2/docs/) - The web framework used
* [Maven](https://maven.apache.org/) - Dependency Management
* [ROME](https://rometools.github.io/rome/) - Used to generate RSS Feeds

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Luca Bolognese** - *Initial work* - [lucabol](https://github.com/lucabol)
* ** Mike Rousos** - *Review* - [mjrousos](https://github.com/mjrousos)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

## Acknowledgments

* Hat tip to anyone who's code was used
* Inspiration
* etc
