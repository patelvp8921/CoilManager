import { DecimalPipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

export interface VisualSlitItem {
  sequenceNo: number;
  width: number;
}

interface LayoutSegment {
  label: string;
  detail: string;
  width: number;
  percent: number;
  cssClass: string;
}

@Component({
  selector: 'app-visual-slitting-layout',
  imports: [DecimalPipe, MatCardModule, MatIconModule],
  templateUrl: './visual-slitting-layout.component.html',
  styleUrl: './visual-slitting-layout.component.scss',
})
export class VisualSlittingLayoutComponent {
  readonly motherCoilWidth = input(0);
  readonly slitItems = input<readonly VisualSlitItem[]>([]);
  readonly leftEdgeTrim = input(0);
  readonly rightEdgeTrim = input(0);
  readonly knifeThickness = input(0);

  protected readonly summary = computed(() => {
    const motherCoilWidth = this.toNumber(this.motherCoilWidth());
    const slitWidths = this.slitItems().map((item) => this.toNumber(item.width));
    const totalSlitWidth = slitWidths.reduce((total, width) => total + width, 0);
    const knifeLoss = Math.max(slitWidths.length - 1, 0) * this.toNumber(this.knifeThickness());
    const edgeTrimTotal = this.toNumber(this.leftEdgeTrim()) + this.toNumber(this.rightEdgeTrim());
    const totalPlannedWidth = totalSlitWidth + knifeLoss + edgeTrimTotal;
    const remainingWidth = Math.max(motherCoilWidth - totalPlannedWidth, 0);
    const excessWidth = Math.max(totalPlannedWidth - motherCoilWidth, 0);
    const utilizationPercent = motherCoilWidth > 0 ? totalSlitWidth / motherCoilWidth * 100 : 0;

    return {
      motherCoilWidth,
      totalSlitWidth,
      knifeLoss,
      edgeTrimTotal,
      totalPlannedWidth,
      remainingWidth,
      excessWidth,
      utilizationPercent,
      isInvalid: excessWidth > 0,
    };
  });

  protected readonly segments = computed<readonly LayoutSegment[]>(() => {
    const summary = this.summary();
    const scaleWidth = Math.max(summary.motherCoilWidth, summary.totalPlannedWidth, 1);
    const segments: LayoutSegment[] = [];

    this.addSegment(segments, 'Left Edge', this.toNumber(this.leftEdgeTrim()), scaleWidth, 'edge');

    this.slitItems().forEach((slit, index) => {
      this.addSegment(segments, `Slit ${slit.sequenceNo}`, this.toNumber(slit.width), scaleWidth, 'slit');

      if (index < this.slitItems().length - 1) {
        this.addSegment(segments, '', this.toNumber(this.knifeThickness()), scaleWidth, 'knife');
      }
    });

    this.addSegment(segments, 'Right Edge', this.toNumber(this.rightEdgeTrim()), scaleWidth, 'edge');
    this.addSegment(segments, 'Remaining', summary.remainingWidth, scaleWidth, 'remaining');
    this.addSegment(segments, 'Excess', summary.excessWidth, scaleWidth, 'excess');

    return segments;
  });

  private addSegment(segments: LayoutSegment[], label: string, width: number, scaleWidth: number, cssClass: string): void {
    if (width <= 0) {
      return;
    }

    segments.push({
      label,
      detail: cssClass === 'knife' ? '' : `${this.formatWidth(width)} mm`,
      width,
      percent: width / scaleWidth * 100,
      cssClass,
    });
  }

  private toNumber(value: number | null | undefined): number {
    return Number(value || 0);
  }

  private formatWidth(width: number): string {
    return Number.isInteger(width) ? width.toString() : width.toFixed(3).replace(/0+$/, '').replace(/\.$/, '');
  }
}
