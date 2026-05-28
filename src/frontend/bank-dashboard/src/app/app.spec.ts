import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { fakeAsync, tick } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);

    fixture.detectChanges();
    http.expectOne('/api/bank-balances/filters').flush({
      banks: [],
      currencies: [],
      balanceTypes: [],
      statuses: [],
    });
    http.expectOne('/api/bank-balances?page=1&pageSize=50&sortBy=date&sortDirection=desc').flush({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 50,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
      summary: {
        totalCount: 0,
        bankCount: 0,
        latestDate: null,
        totalAmountByCurrency: {},
      },
    });

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Dashboard יתרות בנק');
  });

  it('should debounce free text search', fakeAsync(() => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);

    fixture.detectChanges();
    http.expectOne('/api/bank-balances/filters').flush({
      banks: [],
      currencies: [],
      balanceTypes: [],
      statuses: [],
    });
    http.expectOne('/api/bank-balances?page=1&pageSize=50&sortBy=date&sortDirection=desc').flush({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 50,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
      summary: {
        totalCount: 0,
        bankCount: 0,
        latestDate: null,
        totalAmountByCurrency: {},
      },
    });

    const component = fixture.componentInstance as unknown as { scheduleSearch: (search: string) => void };
    component.scheduleSearch('לאומי אופציות');
    fixture.detectChanges();

    tick(499);
    http.expectNone((request) => request.params.get('search') === 'לאומי אופציות');

    tick(1);
    const searchRequest = http.expectOne((request) =>
      request.url === '/api/bank-balances'
      && request.params.get('search') === 'לאומי אופציות'
      && request.params.get('page') === '1'
    );

    expect(searchRequest.request.params.get('search')).toBe('לאומי אופציות');

    searchRequest.flush({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 50,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
      summary: {
        totalCount: 0,
        bankCount: 0,
        latestDate: null,
        totalAmountByCurrency: {},
      },
    });
  }));

  it('should bypass cached responses when refreshing data', () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);
    const component = fixture.componentInstance as unknown as { refreshData: () => void };

    fixture.detectChanges();
    http.expectOne('/api/bank-balances/filters').flush({
      banks: [],
      currencies: [],
      balanceTypes: [],
      statuses: [],
    });
    http.expectOne('/api/bank-balances?page=1&pageSize=50&sortBy=date&sortDirection=desc').flush({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 50,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
      summary: {
        totalCount: 0,
        bankCount: 0,
        latestDate: null,
        totalAmountByCurrency: {},
      },
    });

    component.refreshData();

    const filtersRequest = http.expectOne('/api/bank-balances/filters');
    expect(filtersRequest.request.url).toBe('/api/bank-balances/filters');
    filtersRequest.flush({
      banks: [],
      currencies: [],
      balanceTypes: [],
      statuses: [],
    });

    const balancesRequest = http.expectOne('/api/bank-balances?page=1&pageSize=50&sortBy=date&sortDirection=desc');
    expect(balancesRequest.request.url).toBe('/api/bank-balances');
    balancesRequest.flush({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 50,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
      summary: {
        totalCount: 0,
        bankCount: 0,
        latestDate: null,
        totalAmountByCurrency: {},
      },
    });
  });
});
