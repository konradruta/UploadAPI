# 1. Pobranie repozytorium projektu i przejście do katalogu roboczego
git clone https://github.com/konradruta/UploadAPI
cd project-p6-secure-upload

# 2. Budowa obrazu oraz uruchomienie kontenera z aplikacją Web API
docker compose up --build

## Alternatywne uruchomienie lokalne z użyciem .NET 8 SDK:
dotnet run --urls http://localhost:5000

# 3. Weryfikacja dostępności interfejsów testowych:
- Dokumentacja Swagger UI:   http://localhost:5000/swagger
- Endpoint podatny (BEFORE): POST http://localhost:5000/api/upload-vulnerable
- Endpoint bezpieczny (AFTER): POST http://localhost:5000/api/upload-secure
