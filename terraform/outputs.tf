output "app_service_url" {
  description = "The URL of the deployed app service"
  value       = "https://${azurerm_linux_web_app.main.default_hostname}"
}

output "resource_group_id" {
  description = "The ID of the resource group"
  value       = azurerm_resource_group.main.id
}

output "app_service_plan_id" {
  description = "The ID of the App Service Plan"
  value       = azurerm_service_plan.main.id
}

output "app_service_id" {
  description = "The ID of the App Service"
  value       = azurerm_linux_web_app.main.id
}

output "app_insights_instrumentation_key" {
  description = "The instrumentation key for Application Insights"
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}

output "cosmos_db_connection_string" {
  description = "The primary connection string for Cosmos DB"
  value       = azurerm_cosmosdb_account.main.connection_strings[0]
  sensitive   = true
}

output "cosmos_db_endpoint" {
  description = "The endpoint of the Cosmos DB account"
  value       = azurerm_cosmosdb_account.main.endpoint
}
