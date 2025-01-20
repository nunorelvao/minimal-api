# challenge-backend 

####  Nuno Relvao

## Description
- This API is using new minimal APIs and REST design using Microsoft .net Core 8.
- It is ready to be Dockerized .
- It has Xunit tests.
- The API has [Scalar](https://github.com/scalar/scalar) opensource documentation and is compatible with newest openapi standards.

# To Run on Docker 
 - go to where the DockerFile is located.
 - run docker command:  ``` docker build -t nunor/challenge .  ``` to build Image.
 - run docker command: ``` docker run nunor/challenge -P  ```to run a container with all exposed ports by default .

# Test API with Dummy Data
- The Swagger Documentation has Description for the endpoints
- please use dummy data for operator:is "001", "002" and "003".
- there is a GET  method **/collisionsforoperator** that is used only for getting dummy data from InMemoryDB, ***this is normally not intended to be exposed!***.
- with data in memory fetched it is easy to copy paste values and alter them to use in POST or PATCH endpoints.
