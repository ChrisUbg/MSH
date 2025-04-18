# create migration
dotnet ef migrations add InitialSchema --project src/MSH.Infrastructure --startup-project src/MSH.Web

# remove the last migration
dotnet ef migrations remove --project src/MSH.Infrastructure --startup-project src/MSH.Web

# db update
dotnet ef database update --project src/MSH.Infrastructure --startup-project src/MSH.Web