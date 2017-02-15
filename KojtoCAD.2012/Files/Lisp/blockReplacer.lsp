(defun c:BRE (/ *error* blk f ss temp)
;; Replace multiple instances of selected blocks (can be different) with selected block
;; Size and Rotation will be taken from original block and original will be deleted
;; Required subroutines: AT:GetSel
;; Alan J. Thompson, 02.09.10
;; Found at: http://www.cadtutor.net/forum/showthread.php?48458-Replace-Selected-Block-Or-Blocks-With-Another-Block
(vl-load-com)
(defun *error* (msg)
(and f *AcadDoc* (vla-endundomark *AcadDoc*))
(if (and msg (not (wcmatch (strcase msg) "*BREAK*,*CANCEL*,*QUIT*,")))
(princ (strcat "\nError: " msg))
)
)
(if
(and
(AT:GetSel
entsel
"\nSelect replacement block: "
(lambda (x / e)
(if
(and
(eq "INSERT" (cdr (assoc 0 (setq e (entget (car x))))))
(/= 4 (logand (cdr (assoc 70 (tblsearch "BLOCK" (cdr (assoc 2 e))))) 4))
(/= 4 (logand (cdr (assoc 70 (entget (tblobjname "LAYER" (cdr (assoc 8 e)))))) 4))
)
(setq blk (vlax-ename->vla-object (car x)))
)
)
)
(princ "\nSelect blocks to be repalced: ")
(setq ss (ssget "_:L" '((0 . "INSERT"))))
)
(progn
(setq f (not (vla-startundomark
(cond (*AcadDoc*)
((setq *AcadDoc* (vla-get-activedocument (vlax-get-acad-object))))
)
)
)
)
(vlax-for x (setq ss (vla-get-activeselectionset *AcadDoc*))
(setq temp (vla-copy blk))
(mapcar (function (lambda (p)
(vl-catch-all-apply
(function vlax-put-property)
(list temp p (vlax-get-property x p))
)
)
)
'(Insertionpoint Rotation XEffectiveScaleFactor YEffectiveScaleFactor
ZEffectiveScaleFactor
)
)
(vla-delete x)
)
(vla-delete ss)
(*error* nil)
)
)
(princ)
)
(defun AT:GetSel (meth msg fnc / ent good)
;; meth - selection method (entsel, nentsel, nentselp)
;; msg - message to display (nil for default)
;; fnc - optional function to apply to selected object
;; Ex: (AT:GetSel entsel "\nSelect arc: " (lambda (x) (eq (cdr (assoc 0 (entget (car x)))) "ARC")))
;; Alan J. Thompson, 05.25.10
(setvar 'errno 0)
(while (not good)
(setq ent (meth (cond (msg)
("\nSelect object: ")
)
)
)
(cond
((vl-consp ent)
(setq good (cond ((or (not fnc) (fnc ent)) ent)
((prompt "\nInvalid object!"))
)
)
)
((eq (type ent) 'STR) (setq good ent))
((setq good (eq 52 (getvar 'errno))) nil)
((eq 7 (getvar 'errno)) (setq good (prompt "\nMissed, try again.")))
)
)
)