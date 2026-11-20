import { CategoriaDto } from './categoria.model';
import { StatoCopia } from './enums';

export interface LibroSummaryDto {
  id: string;
  titolo: string;
  autore: string;
  categorie: CategoriaDto[];
  anno: number | null;
  copieDisponibili: number;
  copieTotali: number;
}

export interface CopiaLibroDto {
  id: string;
  proprietarioId: string;
  proprietarioNickname: string;
  stato: StatoCopia;
  percorsoCopertina: string | null;
  percorsoCopertinaThumbnail: string | null;
  dataDisponibilitaStimata: string | null;
}

export interface LibroDetailDto {
  id: string;
  titolo: string;
  autore: string;
  categorie: CategoriaDto[];
  anno: number | null;
  isbn: string | null;
  mediaVotoRecensioni: number | null;
  numeroRecensioni: number;
  /** Aperture della scheda, metrica d'uso aggregata per giorno lato server. */
  numeroVisualizzazioni: number;
  copie: CopiaLibroDto[];
}

export interface LibroDto {
  id: string;
  titolo: string;
  autore: string;
  categorie: CategoriaDto[];
  anno: number | null;
  isbn: string | null;
}

export interface CreaLibroRequest {
  titolo: string;
  autore: string;
  /** Identificativi delle categorie scelte dal vocabolario: da 1 a 5. */
  categorieIds: string[];
  anno: number | null;
  isbn: string | null;
}

export type AggiornaLibroRequest = CreaLibroRequest;

export interface MiaCopiaLibroDto {
  id: string;
  libroId: string;
  titolo: string;
  autore: string;
  categorie: CategoriaDto[];
  anno: number | null;
  stato: StatoCopia;
  percorsoCopertina: string | null;
  percorsoCopertinaThumbnail: string | null;
  dataDisponibilitaStimata: string | null;
  /** Hub del proprietario presso cui la copia e' effettivamente disponibile. */
  hubDisponibili: number;
  /** Hub totali registrati dal proprietario. */
  hubTotali: number;
}
