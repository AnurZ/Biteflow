export interface ActivationDraftDto {
  id: number;
  restaurantName: string;
  domain: string;
  ownerFullName: string;
  ownerEmail: string;
  ownerPhone: string;
  address: string;
  city: string;
  state: string;
  status: number;
}

export interface CreateDraftCommand {
  restaurantName?: string;
  domain?: string;
  ownerFullName?: string;
  ownerEmail?: string;
  ownerPhone?: string;
  address?: string;
  city?: string;
  state?: string;
}

export interface UpdateDraftCommand {
  id: number;
  restaurantName: string;
  domain: string;
  ownerFullName: string;
  ownerEmail: string;
  ownerPhone: string;
  address: string;
  city: string;
  state: string;
}

export interface PageResult<T> {
  total: number;
  items: T[];
}

export interface ConfirmActivationResult {
  tenantId: string;
  restaurantName: string;
  adminUsername: string;
  adminPassword: string;
}
