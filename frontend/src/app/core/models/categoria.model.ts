/** Voce del vocabolario controllato, come compare accanto a un'opera. */
export interface CategoriaDto {
  id: string;
  nome: string;
}

/** Voce del vocabolario nella schermata di gestione Admin. */
export interface CategoriaAdminDto extends CategoriaDto {
  /** Opere che usano la categoria. */
  numeroLibri: number;
  /** Proposte non ancora valutate che la usano: bloccano anch'esse l'eliminazione. */
  numeroProposte: number;
}

export interface SalvaCategoriaRequest {
  nome: string;
}
