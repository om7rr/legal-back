variable "environment" {
  description = "Deployment environment (staging|production)."
  type        = string
}

variable "region" {
  description = "KSA cloud region (must be live and CST-licensed for the data classification). Set after ADR-0003."
  type        = string
  default     = ""
}

variable "project" {
  description = "Resource name prefix."
  type        = string
  default     = "legalplatform"
}
