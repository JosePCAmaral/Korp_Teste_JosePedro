import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { NotasFiscaisComponent } from './notas-fiscais.component';

describe('NotasFiscaisComponent', () => {
  let component: NotasFiscaisComponent;
  let fixture: ComponentFixture<NotasFiscaisComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotasFiscaisComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotasFiscaisComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
