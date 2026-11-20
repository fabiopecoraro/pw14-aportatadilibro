import { CategoriaDto } from './categoria.model';

export interface HubDisponibilitaDto {
  hubId: string;
  nome: string;
  indirizzo: string;
  lat: number;
  lng: number;
  copieDisponibili: number;
}

export interface RicercaLibroDto {
  libroId: string;
  titolo: string;
  autore: string;
  categorie: CategoriaDto[];
  anno: number | null;
  hubDisponibili: HubDisponibilitaDto[];
}
