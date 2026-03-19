import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ProceduresListComponent } from './procedures-list.component';
import { ProcedureService, ProcedureDto, ProcedureImportDto } from '../data-access/procedure.service';

describe('ProceduresListComponent', () => {
  let procedureService: jest.Mocked<Pick<ProcedureService, 'getProcedures'>>;

  const mockProcedures: ProcedureDto[] = [
    {
      id: 'proc-1',
      code: '10101012',
      description: 'Consulta em Clínica Médica',
      value: 150.0,
      effectiveFrom: '2025-01-01',
      effectiveTo: null,
      source: 'Manual',
    },
    {
      id: 'proc-2',
      code: '20203015',
      description: 'Eletrocardiograma',
      value: 80.5,
      effectiveFrom: '2025-01-01',
      effectiveTo: '2025-12-31',
      source: 'Tuss',
    },
  ];

  function setup() {
    procedureService = { getProcedures: jest.fn() };

    TestBed.configureTestingModule({
      imports: [ProceduresListComponent],
      providers: [
        provideRouter([]),
        { provide: ProcedureService, useValue: procedureService },
      ],
    });
  }

  it('renders list of procedures on init', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of(mockProcedures));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('.procedures-list__row');
    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('Consulta em Clínica Médica');
    expect(rows[1].textContent).toContain('Eletrocardiograma');
  }));

  it('renders vigência and fonte columns', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of(mockProcedures));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('.procedures-list__row');
    expect(rows[0].textContent).toContain('2025-01-01');
    expect(rows[0].textContent).toContain('em vigor');
    expect(rows[0].textContent).toContain('Manual');
    expect(rows[1].textContent).toContain('2025-12-31');
    expect(rows[1].textContent).toContain('Tuss');
  }));

  it('shows empty state when no procedures', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of([]));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.procedures-list__empty')).toBeTruthy();
  }));

  it('shows error message on load failure', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(throwError(() => new Error('fail')));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
  }));

  it('opens form in create mode when "Novo procedimento" clicked', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of([]));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const btn = fixture.nativeElement.querySelector('.procedures-list__header .btn--primary');
    btn.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.formOpen()).toBe(true);
    expect(fixture.componentInstance.selectedProcedure()).toBeNull();
  }));

  it('opens import panel when "Importar" button clicked', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of([]));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const btn = fixture.nativeElement.querySelector('.procedures-list__header .btn--secondary');
    btn.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.importOpen()).toBe(true);
  }));

  it('opens form in edit mode when row clicked', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of(mockProcedures));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const row = fixture.nativeElement.querySelector('.procedures-list__row');
    row.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.formOpen()).toBe(true);
    expect(fixture.componentInstance.selectedProcedure()?.id).toBe('proc-1');
  }));

  it('closes form and resets selectedProcedure on closeForm', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of(mockProcedures));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.openEditForm(mockProcedures[0]);
    fixture.detectChanges();
    expect(fixture.componentInstance.formOpen()).toBe(true);

    fixture.componentInstance.closeForm();
    fixture.detectChanges();
    expect(fixture.componentInstance.formOpen()).toBe(false);
    expect(fixture.componentInstance.selectedProcedure()).toBeNull();
  }));

  it('adds new procedure to list on onSaved with new procedure', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of(mockProcedures));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const newProcedure: ProcedureDto = {
      id: 'proc-3',
      code: '30304020',
      description: 'Radiografia de Tórax',
      value: 60.0,
      effectiveFrom: '2025-01-01',
      effectiveTo: null,
      source: 'Manual',
    };

    fixture.componentInstance.onSaved(newProcedure);
    expect(fixture.componentInstance.procedures()).toHaveLength(3);
    expect(fixture.componentInstance.procedures()[2].id).toBe('proc-3');
  }));

  it('updates existing procedure in list on onSaved with existing id', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of(mockProcedures));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    const updated: ProcedureDto = { ...mockProcedures[0], value: 200.0 };
    fixture.componentInstance.onSaved(updated);

    expect(fixture.componentInstance.procedures()).toHaveLength(2);
    expect(fixture.componentInstance.procedures()[0].value).toBe(200.0);
  }));

  it('closes form after onSaved', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of(mockProcedures));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.openCreateForm();
    expect(fixture.componentInstance.formOpen()).toBe(true);

    fixture.componentInstance.onSaved(mockProcedures[0]);
    expect(fixture.componentInstance.formOpen()).toBe(false);
  }));

  it('calls getProcedures with activeOnly=false when toggling all vigências', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of(mockProcedures));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    expect(procedureService.getProcedures).toHaveBeenCalledWith(true);

    fixture.componentInstance.toggleVigencias();
    tick();

    expect(procedureService.getProcedures).toHaveBeenCalledWith(false);
  }));

  it('closes import panel and reloads on onImported', fakeAsync(() => {
    setup();
    procedureService.getProcedures.mockReturnValue(of(mockProcedures));
    const fixture = TestBed.createComponent(ProceduresListComponent);
    fixture.detectChanges();
    tick();
    fixture.detectChanges();

    fixture.componentInstance.openImport();
    expect(fixture.componentInstance.importOpen()).toBe(true);

    const importDto: ProcedureImportDto = {
      id: 'imp-1',
      source: 'Tuss',
      effectiveFrom: '2025-01-01',
      totalRows: 10,
      importedRows: 10,
      skippedRows: 0,
      errorSummary: null,
      createdAt: new Date().toISOString(),
    };
    fixture.componentInstance.onImported(importDto);
    tick();

    expect(fixture.componentInstance.importOpen()).toBe(false);
  }));
});
