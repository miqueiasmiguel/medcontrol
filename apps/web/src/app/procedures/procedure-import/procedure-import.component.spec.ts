import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ProcedureImportComponent } from './procedure-import.component';
import { ProcedureService, ProcedureImportDto } from '../data-access/procedure.service';

describe('ProcedureImportComponent', () => {
  let procedureService: jest.Mocked<Pick<ProcedureService, 'importProcedures'>>;

  const mockImportDto: ProcedureImportDto = {
    id: 'import-1',
    source: 'Tuss',
    effectiveFrom: '2025-01-01',
    totalRows: 100,
    importedRows: 95,
    skippedRows: 5,
    errorSummary: null,
    createdAt: new Date().toISOString(),
  };

  function setup() {
    procedureService = { importProcedures: jest.fn() };

    TestBed.configureTestingModule({
      imports: [ProcedureImportComponent],
      providers: [
        provideRouter([]),
        { provide: ProcedureService, useValue: procedureService },
      ],
    });
  }

  it('renders the import form', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureImportComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('select[id="source"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('input[id="effectiveFrom"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('input[id="file"]')).toBeTruthy();
  });

  it('marks fields as touched on invalid submit', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureImportComponent);
    fixture.detectChanges();

    fixture.componentInstance.submit();

    expect(fixture.componentInstance.form.touched).toBe(true);
    expect(procedureService.importProcedures).not.toHaveBeenCalled();
  });

  it('shows file required error when form is valid but no file selected', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureImportComponent);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({ source: 'Tuss', effectiveFrom: '2025-01-01' });
    fixture.componentInstance.selectedFile = null;
    fixture.componentInstance.submit();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toContain('arquivo');
    expect(procedureService.importProcedures).not.toHaveBeenCalled();
  });

  it('calls importProcedures and emits imported on success', fakeAsync(() => {
    setup();
    procedureService.importProcedures.mockReturnValue(of(mockImportDto));

    const fixture = TestBed.createComponent(ProcedureImportComponent);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({ source: 'Tuss', effectiveFrom: '2025-01-01' });
    fixture.componentInstance.selectedFile = new File([''], 'test.csv', { type: 'text/csv' });

    const importedSpy = jest.spyOn(fixture.componentInstance.imported, 'emit');
    fixture.componentInstance.submit();
    tick();

    expect(procedureService.importProcedures).toHaveBeenCalledWith(
      fixture.componentInstance.selectedFile,
      'Tuss',
      '2025-01-01',
    );
    expect(importedSpy).toHaveBeenCalledWith(mockImportDto);
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('shows import result summary on success', fakeAsync(() => {
    setup();
    procedureService.importProcedures.mockReturnValue(of(mockImportDto));

    const fixture = TestBed.createComponent(ProcedureImportComponent);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({ source: 'Tuss', effectiveFrom: '2025-01-01' });
    fixture.componentInstance.selectedFile = new File([''], 'test.csv', { type: 'text/csv' });

    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    const result = fixture.nativeElement.querySelector('.import-result');
    expect(result).toBeTruthy();
    expect(result.textContent).toContain('95');
    expect(result.textContent).toContain('5');
  }));

  it('shows generic error on import failure', fakeAsync(() => {
    setup();
    procedureService.importProcedures.mockReturnValue(throwError(() => new Error('fail')));

    const fixture = TestBed.createComponent(ProcedureImportComponent);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({ source: 'Tuss', effectiveFrom: '2025-01-01' });
    fixture.componentInstance.selectedFile = new File([''], 'test.csv', { type: 'text/csv' });

    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('emits closed when close() is called', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureImportComponent);
    fixture.detectChanges();

    const closedSpy = jest.spyOn(fixture.componentInstance.closed, 'emit');
    fixture.componentInstance.close();

    expect(closedSpy).toHaveBeenCalled();
  });
});
