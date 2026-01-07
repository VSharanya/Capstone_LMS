export interface LoginRequest {
  Email: string;
  Password: string;
}

export interface RegisterRequest {
  FullName: string;
  Email: string;
  Password: string;
  role: 'Customer';
  annualIncome: number;
}

export interface LoginResponse {
  token: string;
}
