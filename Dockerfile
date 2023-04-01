#Build image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env 

WORKDIR /src
COPY ./Shop ./Shop
COPY ./Shop.ComponentTests ./Shop.ComponentTests

RUN mv ./Shop/appsettings.Docker.json ./Shop/appsettings.Development.json

RUN cat ./Shop/appsettings.Development.json

#issue https://github.com/dotnet/aspnetcore/issues/46218 WORKDIR ./Shop
WORKDIR ./Shop
RUN mkdir ./jwts -p

RUN TOKEN_CUSTOMER=$(dotnet user-jwts create --name Customer --output token) \
		&& TOKEN_ACCOUNTING=$(dotnet user-jwts create --name Accounting --role accounting --output token) \
		&& echo "{\"TestJwtTokens\":{\"Customer\":\"${TOKEN_CUSTOMER}\", \"Accounting\":\"${TOKEN_ACCOUNTING}\"}}" > ./jwts/tokens.json

WORKDIR /src
RUN cat ./Shop/jwts/tokens.json

RUN mkdir /published/Web/secrets -p
RUN cp ~/.microsoft/usersecrets /published/Web/secrets -r
RUN dotnet publish ./Shop/Shop.csproj -c Release -o /published/Web -r linux-x64 --self-contained true /p:PublishReadyToRun=true
RUN dotnet publish ./Shop.ComponentTests/Shop.ComponentTests.csproj -c Release -o /published/tests 
RUN cp ./Shop/jwts/tokens.json /published/tests

#Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as development
WORKDIR /app

COPY --from=build-env /published/Web ./
RUN mkdir ~/.microsoft/usersecrets -p
RUN cp ./secrets/usersecrets/ ~/.microsoft -r

ENTRYPOINT ["./Shop"]

#Test image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS tests 
WORKDIR /src
COPY --from=build-env /published/tests ./
ENTRYPOINT exec dotnet test Shop.ComponentTests.dll