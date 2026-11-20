import { CategoriaDto } from './categoria.model';
import { StatoProposta } from './enums';

export interface CreaPropostaLibroRequest {
  titolo: string;
  autore: string;
  /** Identificativi delle categorie scelte dal vocabolario: da 1 a 5. */
  categorieIds: string[];
  anno: number | null;
  isbn: string | null;
}

export interface PropostaLibroDto {
  id: string;
  titolo: string;
  autore: string;
  categorie: CategoriaDto[];
  anno: number | null;
  isbn: string | null;
  proponenteId: string;
  proponenteNickname: string;
  stato: StatoProposta;
  dataProposta: string;
  dataGestione: string | null;
  libroCreatoId: string | null;
}
