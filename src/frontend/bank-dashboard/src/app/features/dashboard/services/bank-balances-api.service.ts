import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, of, tap } from 'rxjs';

import {
  BankBalanceFilterOptions,
  BankBalanceListResponse,
} from '../models/bank-balance.model';
import { BankBalanceFilterForm } from '../models/bank-balance-filter.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BankBalancesApiService {
  private readonly cacheTtlMs = 3 * 60 * 1000;
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/bank-balances`;
  private readonly responseCache = new Map<string, CacheEntry<unknown>>();

  getBalances(filters: BankBalanceFilterForm): Observable<BankBalanceListResponse> {
    const params = this.toParams(filters);
    const cacheKey = `${this.baseUrl}?${params.toString()}`;
    const cached = this.getFromCache<BankBalanceListResponse>(cacheKey);
    if (cached) {
      return of(cached);
    }

    return this.http.get<BankBalanceListResponse>(this.baseUrl, { params }).pipe(
      tap((response) => this.saveToCache(cacheKey, response))
    );
  }

  getFilterOptions(): Observable<BankBalanceFilterOptions> {
    const url = `${this.baseUrl}/filters`;
    const cached = this.getFromCache<BankBalanceFilterOptions>(url);
    if (cached) {
      return of(cached);
    }

    return this.http.get<BankBalanceFilterOptions>(url).pipe(
      tap((response) => this.saveToCache(url, response))
    );
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
    params = this.appendNumber(params, 'page', filters.page);
    params = this.appendNumber(params, 'pageSize', filters.pageSize);
    params = this.appendString(params, 'sortBy', filters.sortBy);
    params = this.appendString(params, 'sortDirection', filters.sortDirection);

    return params;
  }

  private appendString(params: HttpParams, key: string, value: string): HttpParams {
    const trimmed = value.trim();
    return trimmed ? params.set(key, trimmed) : params;
  }

  private appendNumber(params: HttpParams, key: string, value: number | null): HttpParams {
    return value === null || Number.isNaN(value) ? params : params.set(key, value);
  }

  private getFromCache<T>(key: string): T | null {
    const entry = this.responseCache.get(key);
    if (!entry) {
      return null;
    }

    if (entry.expiresAt <= Date.now()) {
      this.responseCache.delete(key);
      return null;
    }

    return entry.value as T;
  }

  private saveToCache<T>(key: string, value: T): void {
    this.responseCache.set(key, {
      value,
      expiresAt: Date.now() + this.cacheTtlMs,
    });
  }
}

interface CacheEntry<T> {
  value: T;
  expiresAt: number;
}
