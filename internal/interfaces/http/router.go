package http

import "github.com/go-chi/chi/v5"

func RegisterCRMRoutes(r chi.Router, crmHandler *CRMHandler) {
	r.Post("/api/crm/campaigns/{id}/execute", crmHandler.ExecuteCampaign)
}
