import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ProdutoService } from '../../services/produto.service';
import { Produto } from '../../models/produto.model';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatTableModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatCardModule, MatSnackBarModule
  ],
  templateUrl: './produtos.component.html',
  styleUrl: './produtos.component.scss'
})
export class ProdutosComponent implements OnInit {
  private produtoService = inject(ProdutoService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  produtos: Produto[] = [];
  colunas = ['codigo', 'descricao', 'saldo'];

  form = this.fb.group({
    codigo: ['', Validators.required],
    descricao: ['', Validators.required],
    saldo: [0, [Validators.required, Validators.min(0)]]
  });

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.produtoService.getAll().subscribe({
      next: (produtos) => this.produtos = produtos,
      error: () => this.snackBar.open('Erro ao carregar produtos.', 'Fechar', { duration: 3000 })
    });
  }

  salvar(): void {
    if (this.form.invalid) return;

    this.produtoService.create(this.form.value as Produto).subscribe({
      next: () => {
        this.snackBar.open('Produto cadastrado com sucesso!', 'Fechar', { duration: 3000 });
        this.form.reset({ saldo: 0 });
        this.carregarProdutos();
      },
      error: (err) => {
        const mensagem = err.error?.errors
          ? Object.values(err.error.errors).flat().join(' ')
          : 'Erro ao cadastrar produto.';
        this.snackBar.open(mensagem, 'Fechar', { duration: 4000 });
      }
    });
  }
}