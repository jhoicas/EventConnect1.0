package crm

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"net/http"
	"time"
)

const CampaignStatusSending = "ENVIANDO"

type CampaignRepository interface {
	UpdateStatus(ctx context.Context, campaignID string, status string) error
}

type CampaignUseCase struct {
	repo       CampaignRepository
	httpClient *http.Client
	triggerURL string
}

func NewCampaignUseCase(repo CampaignRepository, triggerURL string) *CampaignUseCase {
	return &CampaignUseCase{
		repo: repo,
		httpClient: &http.Client{
			Timeout: 10 * time.Second,
		},
		triggerURL: triggerURL,
	}
}

// ExecuteCampaign updates campaign status to ENVIANDO and triggers Azure Function.
func (u *CampaignUseCase) ExecuteCampaign(ctx context.Context, campaignID string) error {
	if campaignID == "" {
		return errors.New("campaignID is required")
	}
	if u.triggerURL == "" {
		return errors.New("azure campaign trigger URL is not configured")
	}

	if err := u.repo.UpdateStatus(ctx, campaignID, CampaignStatusSending); err != nil {
		return err
	}

	// Fire-and-forget to avoid blocking frontend response.
	go u.triggerAzureFunction(campaignID)

	return nil
}

func (u *CampaignUseCase) triggerAzureFunction(campaignID string) {
	payload := map[string]string{
		"campaign_id": campaignID,
	}

	body, err := json.Marshal(payload)
	if err != nil {
		return
	}

	req, err := http.NewRequest(http.MethodPost, u.triggerURL, bytes.NewReader(body))
	if err != nil {
		return
	}
	req.Header.Set("Content-Type", "application/json")

	_, _ = u.httpClient.Do(req)
}
