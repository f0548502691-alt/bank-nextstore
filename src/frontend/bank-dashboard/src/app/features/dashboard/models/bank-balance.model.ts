export interface BankBalance {
  id: number;
  date: string;
  bankName: string;
  accountNumber: string;
  balanceType: string;
  currency: string;
  amount: number;
  status: string;
}

export interface BankBalanceSummary {
  totalCount: number;
  bankCount: number;
  latestDate: string | null;
  totalAmountByCurrency: Record<string, number>;
}

export interface BankBalanceListResponse {
  items: BankBalance[];
  totalCount: number;
  summary: BankBalanceSummary;
}

export interface BankBalanceFilterOptions {
  banks: string[];
  currencies: string[];
  balanceTypes: string[];
  statuses: string[];
}
