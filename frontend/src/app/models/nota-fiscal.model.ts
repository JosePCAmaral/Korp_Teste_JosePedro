export interface ItemNotaFiscal {
  id?: number;
  produtoId: number;
  produtoCodigo?: string;
  produtoDescricao?: string;
  quantidade: number;
}

export interface NotaFiscal {
  id?: number;
  numeracao?: number;
  status?: number; // 0 = Aberta, 1 = Fechada
  dataCriacao?: string;
  itens: ItemNotaFiscal[];
}