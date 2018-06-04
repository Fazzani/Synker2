import { Directive, forwardRef, Attribute, Input } from '@angular/core';
import { Validator, AbstractControl, NG_VALIDATORS, FormControl, ValidationErrors } from '@angular/forms';
@Directive({
  selector: '[validateEqual][formControlName],[validateEqual][formControl],[validateEqual][ngModel]',
  providers: [{ provide: NG_VALIDATORS, useExisting: EqualValidator, multi: true }]
})
export class EqualValidator implements Validator {

  constructor(
    @Attribute('validateEqual') public validateEqual: string,
    @Attribute('reverse') public reverse: string) {

  }

  validate(c: FormControl): ValidationErrors {
    // self value
    let v = c.value;
    // control value
    let e = c.root.get(this.validateEqual);
    // value not equal
    if (e && v !== e.value && !this.isReverse) return {
      validateEqual: false
    }
    // value equal and reverse
    if (e && v === e.value && this.isReverse) {
      delete e.errors['validateEqual'];
      if (!Object.keys(e.errors).length) e.setErrors(null);
    }
    // value not equal and reverse
    if (e && v !== e.value && this.isReverse) {
      e.setErrors({ validateEqual: false })
    }
    return null;
  }

  private get isReverse() {
    if (!this.reverse) return false;
    return this.reverse === 'true' ? true : false;
  }
}

