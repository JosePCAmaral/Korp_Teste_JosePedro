import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroupDirective, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { NotaFiscalService } from '../../services/nota-fiscal.service';
import { ProdutoService } from '../../services/produto.service';
import { Produto } from '../../models/produto.model';
import { NotaFiscal, ItemNotaFiscal } from '../../models/nota-fiscal.model';

@Component({
  selector: 'app-notas-fiscais',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatTableModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule
  ],
  templateUrl: './notas-fiscais.component.html',
  styleUrl: './notas-fiscais.component.scss'
})
export class NotasFiscaisComponent implements OnInit {
  private notaFiscalService = inject(NotaFiscalService);
  private produtoService = inject(ProdutoService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  @ViewChild(FormGroupDirective) formDirective!: FormGroupDirective;

  notas: NotaFiscal[] = [];
  produtos: Produto[] = [];
  itensDaNota: ItemNotaFiscal[] = [];
  colunasNotas = ['numeracao', 'status', 'itens', 'acoes'];
  imprimindoId: number | null = null;
  carregandoNotas = false;
  erroAoCarregarNotas = false;
  criando = false;

  itemForm = this.fb.group({
    produtoId: [null as number | null, Validators.required],
    quantidade: [1, [Validators.required, Validators.min(1)]]
  });

  ngOnInit(): void {
    this.carregarProdutos();
    this.carregarNotas();
  }

  carregarProdutos(): void {
    this.produtoService.getAll().subscribe({ next: (p) => this.produtos = p });
  }

  carregarNotas(): void {
    this.carregandoNotas = this.notas.length === 0;
    this.erroAoCarregarNotas = false;
    this.notaFiscalService.getAll()
      .pipe(finalize(() => this.carregandoNotas = false))
      .subscribe({
        next: (n) => this.notas = n,
        error: () => this.erroAoCarregarNotas = true
      });
  }

  adicionarItem(): void {
    if (this.itemForm.invalid) return;
    const { produtoId, quantidade } = this.itemForm.value;
    const produto = this.produtos.find(p => p.id === produtoId);
    if (!produto) return;

    const novoItem: ItemNotaFiscal = {
      produtoId: produto.id!,
      produtoCodigo: produto.codigo,
      produtoDescricao: produto.descricao,
      quantidade: quantidade!
    };

    this.itensDaNota = [...this.itensDaNota, novoItem];

    this.formDirective.resetForm({ produtoId: null, quantidade: 1 });
  }

  removerItem(index: number): void {
    this.itensDaNota = this.itensDaNota.filter((_, i) => i !== index);
  }

  criarNota(): void {
    if (this.itensDaNota.length === 0) {
      this.snackBar.open('Adicione pelo menos um item antes de criar a nota.', 'Fechar', { duration: 3000 });
      return;
    }

    const payload = { itens: this.itensDaNota.map(i => ({ produtoId: i.produtoId, quantidade: i.quantidade })) };

    this.criando = true;
    this.notaFiscalService.create(payload)
      .pipe(finalize(() => this.criando = false))
      .subscribe({
        next: () => {
          this.snackBar.open('Nota fiscal criada com sucesso!', 'Fechar', { duration: 3000 });
          this.itensDaNota = [];
          this.carregarNotas();
        }
      });
  }

  imprimir(nota: NotaFiscal): void {
    this.imprimindoId = nota.id!;

    this.notaFiscalService.imprimir(nota.id!)
      .pipe(finalize(() => this.imprimindoId = null))
      .subscribe({
        next: () => {
          this.snackBar.open(`Nota ${nota.numeracao} impressa com sucesso!`, 'Fechar', { duration: 3000 });
          this.carregarNotas();
          this.carregarProdutos();
        }
      });
  }

  statusLabel(status?: number): string {
    return status === 1 ? 'Fechada' : 'Aberta';
  }
}