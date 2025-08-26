
# iwell-dashboard

## Overview
Welcome to the iwell-dashboard starter project! This is an Angular-based web application, using Bootstrap for styling and the iwell-api for data. 

This project also includes a Dockerfile for easy setup and deployment.

## Prerequisites
Before you begin, make sure you have Docker installed on your system. This project uses Docker to simplify setup and ensure a consistent environment across all machines.

### CORS workaround
To allow for local development access to the api and bypass CORS requirements, this project contains a http proxy.
Requests to the api should use `/myiwell/api`, no need to add the full url host.

## Getting Started

### Building the Docker Image
To build the Docker image, run the following command in the root directory of the project:

`docker build -t iwell-dashboard .`

This command creates a Docker image named iwell-dashboard.

### Running the Application
After building the image, you can start the application with:

`docker run --rm -v ${PWD}:/app -v /app/node_modules -p 4200:4200 --name iwell-dashboard iwell-dashboard ng serve --host 0.0.0.0 --poll 1000`
This command will start the development server and serve the application. The -v ${PWD}:/app flag mounts your current directory into the container, allowing live updates to the codebase.

### Accessing the Application
Once the server is running, you can access the iwell-dashboard at http://localhost:4200.

## Development Workflow
### Making Changes
Any changes you make to the code on your host machine will be reflected in the Docker container, thanks to the volume mount. Simply edit the files as you would in any other development project.

### Stopping the Container
To stop the Docker container, use:

`docker stop iwell-dashboard` 

The container will be removed automatically when stopped, due to the --rm flag.

### Add npm modules
You need to access the shell of your running Docker container. 

`docker exec -it iwell-dashboard /bin/bash`

Then you can install npm modules as usual.
