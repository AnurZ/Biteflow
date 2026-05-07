export interface PageResult<T> {
  total: number;
  items: T[];
}

export interface StaffListItem {
  id: number;
  applicationUserId: string;
  displayName: string;
  email: string;
  firstName: string;
  lastName: string;
  position: string;
  isActive: boolean;
  hireDate?: string;
}

export interface StaffDetails {
  id: number;
  applicationUserId: string;
  displayName: string;
  email: string;
  role: string;
  position: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  hireDate?: string | undefined;
  terminationDate?: string;
  salary?: number;
  hourlyRate?: number;
  employmentType?: string;
  shiftType?: string;
  shiftStart?: string; // "HH:mm:ss"
  shiftEnd?: string;
  averageRating?: number;
  completedOrders?: number;
  monthlyTips?: number;
  isActive: boolean;
  notes?: string;
}

export interface CreateStaffRequest {
  email?: string;
  displayName?: string;
  plainPassword?: string;
  role?: string;

  firstName: string;
  lastName: string;
  phoneNumber?: string;
  hireDate?: string;
  hourlyRate?: number;
  employmentType?: string;
  shiftType?: string;
  shiftStart?: string;
  shiftEnd?: string;
  isActive: boolean;
  notes?: string;
}

export interface UpdateStaffRequest {
  id: number;
  displayName?:string;
  role?: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  hireDate?: string;
  terminationDate?: string;
  salary?: number;
  hourlyRate?: number;
  employmentType?: string;
  shiftType?: string;
  shiftStart?: string;
  shiftEnd?: string;
  isActive: boolean;
  notes?: string;
}
