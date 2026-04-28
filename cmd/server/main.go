package main

import (
	"database/sql"
	"log"
	nethttp "net/http"
	"os"

	"github.com/go-chi/chi/v5"

	appcrm "eventconnect/internal/application/crm"
	infraCRM "eventconnect/internal/infrastructure/crm"
	httpiface "eventconnect/internal/interfaces/http"
	"eventconnect/pkg/config"

	_ "github.com/jackc/pgx/v5/stdlib"
)

func main() {
	cfg := config.Load()
	if cfg.AzureCampaignTriggerURL == "" {
		log.Println("warning: AZURE_CAMPAIGN_TRIGGER_URL is empty")
	}
	if cfg.DatabaseURL == "" {
		log.Fatal("DATABASE_URL is required")
	}

	db, err := sql.Open("pgx", cfg.DatabaseURL)
	if err != nil {
		log.Fatalf("failed to open database: %v", err)
	}
	defer db.Close()

	if err := db.Ping(); err != nil {
		log.Fatalf("failed to connect database: %v", err)
	}

	repo := infraCRM.NewCampaignRepository(db)
	usecase := appcrm.NewCampaignUseCase(repo, cfg.AzureCampaignTriggerURL)
	handler := httpiface.NewCRMHandler(usecase)

	router := chi.NewRouter()
	httpiface.RegisterCRMRoutes(router, handler)

	addr := ":8080"
	if p := os.Getenv("PORT"); p != "" {
		addr = ":" + p
	}

	log.Printf("server listening on %s", addr)
	if err := nethttp.ListenAndServe(addr, router); err != nil {
		log.Fatal(err)
	}
}
