import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ProcedureFormComponent } from './procedure-form.component';
import { ProcedureService, ProcedureDto } from '../data-access/procedure.service';

describe('ProcedureFormComponent', () => {
  let procedureService: jest.Mocked<Pick<ProcedureService, 'createProcedure' | 'updateProcedure'>>;

  const mockProcedure: ProcedureDto = {
    id: 'proc-1',
    code: '10101012',
    description: 'Consulta em Clínica Médica',
    value: 150.0,
    effectiveFrom: '2025-01-01',
    effectiveTo: null,
    source: 'Manual',
  };

  function setup() {
    procedureService = {
      createProcedure: jest.fn(),
      updateProcedure: jest.fn(),
    };

    TestBed.configureTestingModule({
      imports: [ProcedureFormComponent],
      providers: [
        provideRouter([]),
        { provide: ProcedureService, useValue: procedureService },
      ],
    });
  }

  it('renders in create mode when procedure input is null', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', null);
    fixture.detectChanges();

    expect(fixture.componentInstance.isEditing).toBe(false);
    expect(fixture.nativeElement.querySelector('[type="submit"]').textContent).toContain(
      'Cadastrar procedimento',
    );
  });

  it('renders in edit mode when procedure input is provided', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', mockProcedure);
    fixture.detectChanges();

    expect(fixture.componentInstance.isEditing).toBe(true);
    expect(fixture.nativeElement.querySelector('[type="submit"]').textContent).toContain(
      'Salvar alterações',
    );
  });

  it('patches form values from procedure input including effectiveFrom and effectiveTo', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', mockProcedure);
    fixture.detectChanges();

    const { form } = fixture.componentInstance;
    expect(form.value.code).toBe('10101012');
    expect(form.value.description).toBe('Consulta em Clínica Médica');
    expect(form.value.value).toBe(150.0);
    expect(form.value.effectiveFrom).toBe('2025-01-01');
    expect(form.value.effectiveTo).toBe('');
  });

  it('marks all fields as touched and does not submit when form is invalid', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', null);
    fixture.detectChanges();

    fixture.componentInstance.submit();

    expect(fixture.componentInstance.form.touched).toBe(true);
    expect(procedureService.createProcedure).not.toHaveBeenCalled();
  });

  it('calls createProcedure with effectiveFrom and emits saved on success', fakeAsync(() => {
    setup();
    procedureService.createProcedure.mockReturnValue(of(mockProcedure));

    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({
      code: '10101012',
      description: 'Consulta em Clínica Médica',
      value: 150.0,
      effectiveFrom: '2025-01-01',
      effectiveTo: '',
    });

    const savedSpy = jest.spyOn(fixture.componentInstance.saved, 'emit');
    fixture.componentInstance.submit();
    tick();

    expect(procedureService.createProcedure).toHaveBeenCalledWith({
      code: '10101012',
      description: 'Consulta em Clínica Médica',
      value: 150.0,
      effectiveFrom: '2025-01-01',
      effectiveTo: undefined,
    });
    expect(savedSpy).toHaveBeenCalledWith(mockProcedure);
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('calls updateProcedure without effectiveFrom when editing', fakeAsync(() => {
    setup();
    const updated = { ...mockProcedure, value: 200.0 };
    procedureService.updateProcedure.mockReturnValue(of(updated));

    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', mockProcedure);
    fixture.detectChanges();

    fixture.componentInstance.form.patchValue({ value: 200.0 });

    const savedSpy = jest.spyOn(fixture.componentInstance.saved, 'emit');
    fixture.componentInstance.submit();
    tick();

    expect(procedureService.updateProcedure).toHaveBeenCalledWith(
      'proc-1',
      expect.objectContaining({ value: 200.0 }),
    );
    expect(procedureService.updateProcedure).toHaveBeenCalledWith(
      'proc-1',
      expect.not.objectContaining({ effectiveFrom: expect.anything() }),
    );
    expect(savedSpy).toHaveBeenCalledWith(updated);
  }));

  it('shows duplicate code error on 409 response', fakeAsync(() => {
    setup();
    const error = new HttpErrorResponse({ status: 409 });
    procedureService.createProcedure.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({
      code: '10101012',
      description: 'Consulta',
      value: 100.0,
      effectiveFrom: '2025-01-01',
      effectiveTo: '',
    });
    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toContain('código');
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('shows generic error on non-409 failure', fakeAsync(() => {
    setup();
    const error = new HttpErrorResponse({ status: 500 });
    procedureService.createProcedure.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', null);
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({
      code: '10101012',
      description: 'Consulta',
      value: 100.0,
      effectiveFrom: '2025-01-01',
      effectiveTo: '',
    });
    fixture.componentInstance.submit();
    tick();
    fixture.detectChanges();

    expect(fixture.componentInstance.errorMessage()).toBeTruthy();
    expect(fixture.componentInstance.loading()).toBe(false);
  }));

  it('emits closed when close() is called', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', null);
    fixture.detectChanges();

    const closedSpy = jest.spyOn(fixture.componentInstance.closed, 'emit');
    fixture.componentInstance.close();

    expect(closedSpy).toHaveBeenCalled();
  });

  it('validates value=0 as invalid', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', null);
    fixture.detectChanges();

    const valueControl = fixture.componentInstance.form.controls.value;
    valueControl.setValue(0);
    valueControl.markAsTouched();

    expect(valueControl.invalid).toBe(true);
    expect(valueControl.errors?.['min']).toBeTruthy();
  });

  it('validates value > 0 as valid', () => {
    setup();
    const fixture = TestBed.createComponent(ProcedureFormComponent);
    fixture.componentRef.setInput('procedure', null);
    fixture.detectChanges();

    const valueControl = fixture.componentInstance.form.controls.value;
    valueControl.setValue(0.01);

    expect(valueControl.valid).toBe(true);
  });
});
