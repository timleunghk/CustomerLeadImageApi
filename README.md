# Customer Lead Image API

ASP.NET Core 8 Web API for managing customers and their images.  
Images are stored in Base64 format in a SQLite database.  
Each customer can have up to 10 images.

## Requirements

- .NET 8 SDK

## Run

dotnet restore  
dotnet run  

API runs at:  
https://localhost:5272  

Swagger UI:  
https://localhost:5272/swagger  

## Endpoints

POST /api/customers ¡X create customer with images  

GET /api/customers ¡X get all customers  

GET /api/customers/{id} ¡X get one customer  

GET /api/customers/{id}/images/count ¡X count images  

POST /api/customers/{id}/images ¡X add images  

PUT /api/customers/{id}/images ¡X replace images  

GET /api/customers/{id}/images/{imageId}/preview ¡X preview image  

DELETE /api/customers/{id}/images ¡X delete all images

GET /api/customers/{id}/images/count - count how many images storing per customer

## Video Demonstration
[▶️ Watch Demo Video](https://raw.githubusercontent.com/timleunghk/CustomerLeadImageApi/master/CustomerLeadImageAPIDemo.mp4)
