package config

import "os"

type Config struct {
	AzureCampaignTriggerURL string
	DatabaseURL             string
}

func Load() Config {
	return Config{
		AzureCampaignTriggerURL: os.Getenv("AZURE_CAMPAIGN_TRIGGER_URL"),
		DatabaseURL:             os.Getenv("DATABASE_URL"),
	}
}
