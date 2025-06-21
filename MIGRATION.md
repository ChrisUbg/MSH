# create migration
dotnet ef migrations add Initial --project src/MSH.Infrastructure --startup-project src/MSH.Web

# remove the last migration
dotnet ef migrations remove --project src/MSH.Infrastructure --startup-project src/MSH.Web

# db update
-- dotnet ef database update --project src/MSH.Infrastructure --startup-project src/MSH.Web

-- from outsided connection at the container:
cd src/MSH.Infrastructure
CONNECTION_STRING="Host=localhost;Port=5434;Database=matter_dev;Username=postgres;Password=devpassword" dotnet ef database update

dotnet ef database update --verbose

# vollständiger db reset
  dotnet ef database drop --project src/MSH.Infrastructure --startup-project src/MSH.Web
