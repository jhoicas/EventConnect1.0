package http

import (
	"context"
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
)

type CampaignExecutor interface {
	ExecuteCampaign(ctx context.Context, campaignID string) error
}

type CRMHandler struct {
	usecase CampaignExecutor
}

func NewCRMHandler(usecase CampaignExecutor) *CRMHandler {
	return &CRMHandler{usecase: usecase}
}

func (h *CRMHandler) ExecuteCampaign(w http.ResponseWriter, r *http.Request) {
	campaignID := chi.URLParam(r, "id")
	if campaignID == "" {
		http.Error(w, "campaign id is required", http.StatusBadRequest)
		return
	}

	if err := h.usecase.ExecuteCampaign(r.Context(), campaignID); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusAccepted)
	_ = json.NewEncoder(w).Encode(map[string]string{
		"message": "campaign execution started",
		"status":  "ENVIANDO",
	})
}
