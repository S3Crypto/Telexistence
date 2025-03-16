provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

resource "azurerm_application_insights" "main" {
  name                = "${var.app_service_name}-appinsights"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  application_type    = "web"
}

resource "azurerm_service_plan" "main" {
  name                = var.app_service_plan_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_linux_web_app" "main" {
  name                = var.app_service_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_service_plan.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    always_on     = true
    http2_enabled = true

    application_stack {
      dotnet_version = "8.0"
    }

    cors {
      allowed_origins = var.allowed_origins
    }
  }

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE"       = "1"
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.main.instrumentation_key
    "ConnectionStrings__MongoDB"     = var.mongodb_connection_string
    "MongoDB__DatabaseName"          = var.mongodb_database_name
    "Jwt__Key"                       = var.jwt_key
    "Jwt__Issuer"                    = var.jwt_issuer
    "Jwt__Audience"                  = var.jwt_audience
    "DOCKER_REGISTRY_SERVER_URL"     = "https://index.docker.io"
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_cosmosdb_account" "main" {
  name                = "${var.app_service_name}-cosmos"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  offer_type          = "Standard"
  kind                = "MongoDB"

  capabilities {
    name = "EnableMongo"
  }

  capabilities {
    name = "DisableRateLimitingResponses"
  }

  geo_location {
    location          = azurerm_resource_group.main.location
    failover_priority = 0
  }

  consistency_policy {
    consistency_level       = "Session"
    max_interval_in_seconds = 5
    max_staleness_prefix    = 100
  }
}

resource "azurerm_cosmosdb_mongo_database" "main" {
  name                = var.mongodb_database_name
  resource_group_name = azurerm_resource_group.main.name
  account_name        = azurerm_cosmosdb_account.main.name
}
