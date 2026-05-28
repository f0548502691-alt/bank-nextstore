import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  BankBalanceFilterOptions,
  BankBalanceListResponse,
} from '../models/bank-balance.model';
import { BankBalanceFilterForm } from '../models/bank-balance-filter.model';

@Injectable({ providedIn: 'root' })
export class BankBalancesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/bank-balances';

  getBalances(filters: BankBalanceFilterForm): Observable<BankBalanceListResponse> {
    return this.http.get<BankBalanceListResponse>(this.baseUrl, {
      params: this.toParams(filters),
    });
  }

  getFilterOptions(): Observable<BankBalanceFilterOptions> {
    return this.http.get<BankBalanceFilterOptions>(`${this.baseUrl}/filters`);
  }

  private toParams(filters: BankBalanceFilterForm): HttpParams {
    let params = new HttpParams();

    params = this.appendString(params, 'search', filters.search);
    params = this.appendString(params, 'bankName', filters.bankName);
    params = this.appendString(params, 'currency', filters.currency);
    params = this.appendString(params, 'balanceType', filters.balanceType);
    params = this.appendString(params, 'status', filters.status);
    params = this.appendNumber(params, 'minAmount', filters.minAmount);
    params = this.appendNumber(params, 'maxAmount', filters.maxAmount);

    return params;
  }

  private appendString(params: HttpParams, key: string, value: string): HttpParams {
    const trimmed = value.trim();
    return trimmed ? params.set(key, trimmed) : params;
  }

  private appendNumber(params: HttpParams, key: string, value: number | null): HttpParams {
    return value === null || Number.isNaN(value) ? params : params.set(key, value);
  }
}
