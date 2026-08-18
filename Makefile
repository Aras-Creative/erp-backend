.PHONY: run migrate migrate-address migrate-inventory add-migration clean

export DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER := true

HOST := src/Host/ArasERP.Host

run:
	dotnet run --project $(HOST)

clean:
	dotnet build-server shutdown

migrate: migrate-address migrate-inventory

migrate-address:
	dotnet ef database update --project src/Modules/Address/ArasERP.Modules.Address.Infrastructure --startup-project $(HOST) --context AddressDbContext

migrate-inventory:
	dotnet ef database update --project src/Modules/Inventory/ArasERP.Modules.Inventory.Infrastructure --startup-project $(HOST) --context InventoryDbContext

add-migration:
	dotnet ef migrations add $(name) --project src/Modules/$(module)/ArasERP.Modules.$(module).Infrastructure --startup-project $(HOST) --context $(module)DbContext
