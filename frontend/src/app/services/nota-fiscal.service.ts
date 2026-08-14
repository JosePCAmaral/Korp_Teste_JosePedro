import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { NotaFiscal } from '../models/nota-fiscal.model';

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.faturamentoApiUrl}/notasfiscais`;

  getAll(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.baseUrl);
  }

  create(nota: { itens: { produtoId: number; quantidade: number }[] }): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.baseUrl, nota);
  }

  imprimir(id: number): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(`${this.baseUrl}/${id}/imprimir`, {});
  }
}