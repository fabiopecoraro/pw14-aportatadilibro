import { Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CopiaLibroDto } from '../../../../core/models/catalogo.model';
import { StatoCopia } from '../../../../core/models/enums';
import { BookCover, StatusBadge } from '../../../../shared/ui';
import { resolveFileUrl } from '../../../../core/utils/files';

/**
 * Elenco delle copie di un titolo presenti nel sistema.
 *
 * Non ha un corrispettivo nel design, che immagina un libro per proprietario:
 * e' il punto in cui il catalogo condiviso del progetto richiede una sezione in
 * piu'. Ogni riga mostra la foto della copia fisica (ogni proprietario carica
 * la propria: due copie dello stesso titolo possono avere copertine diverse,
 * usurate o senza sovraccoperta), non un'illustrazione generica del titolo, che
 * e' gia' mostrata in alto nella schermata.
 */
@Component({
  selector: 'app-elenco-copie',
  standalone: true,
  imports: [DatePipe, RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule, BookCover, StatusBadge],
  templateUrl: './elenco-copie.html',
  styleUrl: './elenco-copie.scss'
})
export class ElencoCopie {
  readonly copie = input<CopiaLibroDto[]>([]);

  /** Titolo dell'opera: completa il testo alternativo delle copertine. */
  readonly titolo = input('');

  /** Identificativo dell'utente corrente, per distinguere le proprie copie. */
  readonly utenteId = input<string | null>(null);

  /** Copia su cui e' in corso un'operazione, per mostrarne lo stato di attesa. */
  readonly copiaInAttesa = input<string | null>(null);

  readonly chiediPrestito = output<CopiaLibroDto>();
  readonly cambiaCopertina = output<CopiaLibroDto>();
  readonly rimuovi = output<CopiaLibroDto>();

  readonly StatoCopia = StatoCopia;
  readonly resolveFileUrl = resolveFileUrl;
}
