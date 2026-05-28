export interface BankBalanceFilterForm {
  search: string;
  bankName: string;
  currency: string;
  balanceType: string;
  status: string;
  minAmount: number | null;
  maxAmount: number | null;
}
