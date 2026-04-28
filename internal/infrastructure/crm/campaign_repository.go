package crm

import (
	"context"
	"database/sql"
	"errors"
)

type CampaignRepository struct {
	db *sql.DB
}

func NewCampaignRepository(db *sql.DB) *CampaignRepository {
	return &CampaignRepository{
		db: db,
	}
}

func (r *CampaignRepository) UpdateStatus(ctx context.Context, campaignID string, status string) error {
	if campaignID == "" {
		return errors.New("campaignID is required")
	}

	const q = `UPDATE campaigns SET status = $1, updated_at = NOW() WHERE id = $2`
	res, err := r.db.ExecContext(ctx, q, status, campaignID)
	if err != nil {
		return err
	}

	affected, err := res.RowsAffected()
	if err != nil {
		return err
	}
	if affected == 0 {
		return sql.ErrNoRows
	}

	return nil
}
