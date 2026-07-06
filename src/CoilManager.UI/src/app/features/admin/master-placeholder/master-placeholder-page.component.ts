import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-master-placeholder-page',
  imports: [RouterLink, MatButtonModule, MatCardModule, MatIconModule],
  templateUrl: './master-placeholder-page.component.html',
  styleUrl: './master-placeholder-page.component.scss',
})
export class MasterPlaceholderPageComponent {
  private readonly route = inject(ActivatedRoute);

  protected readonly title = this.route.snapshot.data['title'] as string;
  protected readonly section = this.route.snapshot.data['section'] as string;
  protected readonly createRoute = this.route.snapshot.data['createRoute'] as string | undefined;
}
