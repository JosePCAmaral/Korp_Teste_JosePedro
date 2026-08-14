import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Produto } from '../models/produto.model';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly baseUrl = `${environment.estoqueApiUrl}/produtos`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.baseUrl);
  }

  create(produto: Produto): Observable<Produto> {
    return this.http.post<Produto>(this.baseUrl, produto);
  }
}