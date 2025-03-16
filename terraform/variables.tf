variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
  default     = "westus2"
}

variable "app_service_plan_name" {
  description = "The name of the App Service Plan"
  type        = string
}

variable "app_service_name" {
  description = "The name of the App Service"
  type        = string
}

variable "allowed_origins" {
  description = "List of allowed origins for CORS"
  type        = list(string)
  default     = ["https://localhost:3000"]
}

variable "mongodb_connection_string" {
  description = "MongoDB connection string"
  type        = string
  sensitive   = true
  default     = ""
}

variable "mongodb_database_name" {
  description = "MongoDB database name"
  type        = string
  default     = "TelexistenceDB"
}

variable "jwt_key" {
  description = "Secret key for JWT token generation"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "Issuer for JWT tokens"
  type        = string
  default     = "TelexistenceAPI"
}

variable "jwt_audience" {
  description = "Audience for JWT tokens"
  type        = string
  default     = "TelexistenceClients"
}
