import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-raw-coil-create-page',
  imports: [RouterLink, MatButtonModule, MatCardModule, MatIconModule],
  templateUrl: './raw-coil-create-page.component.html',
  styleUrl: './raw-coil-create-page.component.scss',
})
export class RawCoilCreatePageComponent {}
