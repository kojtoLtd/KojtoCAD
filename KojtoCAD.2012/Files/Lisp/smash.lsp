;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;                       Disclaimer / Warranty                            ;;;
;;;                                                                        ;;;
;;; The information contained herein is being donated to the public domain ;;;
;;; soley for the benefit of licensed AutoCAD END USERS everywhere.  You   ;;;
;;; are free to modify, or reproduce and distribute only in the un-altered ;;;
;;; original form, the information contained herein for ANY NON-COMMERCIAL ;;;
;;; use, provided such reproduction/disribution is NOT FOR PROFIT, and     ;;;
;;; provided that with any such distribution, this NOTICE (or any portion  ;;;
;;; thereof) is NOT REMOVED OR ALTERED from it's original content in ANY   ;;;
;;; manner, shape, or form.                                                ;;;
;;;                                                                        ;;;
;;; Every effort has been made to make this program as complete, accurate  ;;;
;;; and error free as possible.  But no warranty or fitness is implied.    ;;;
;;;                                                                        ;;;
;;; This program is provided on an "as is" basis.  The author shall have   ;;;
;;; neither liability nor responsibility to any person or entity with      ;;;
;;; respect to any loss or damages arising from the use of this program    ;;;
;;; or any portion thereof.                                                ;;;
;;;                                                                        ;;;
;;; Custom AutoLISP programming can be obtained by contacting:             ;;;
;;;                                                                        ;;;
;;;                 Jeffrey S. Pilcher                                     ;;;
;;;                 6913 State Park Road                                   ;;;
;;;                 Spring Grove, IL  60081                                ;;;
;;;                                                                        ;;;
;;;                 (708) 587-5039                                         ;;;
;;;                                                                        ;;;
;;; Or, via electronic medium thru:                                        ;;;
;;;                                                                        ;;;
;;;                 CADtribution  (708) 587-5389                           ;;;
;;;                                                                        ;;;
;;;       CADtribution is an electronic bulletin board system              ;;;
;;;       operated by the 'GREATER CHICAGO AUTOCAD USERS GROUP'            ;;;
;;;                                                                        ;;;
;;;************************************************************************;;;
;;;  IF YOU FIND THIS PROGRAM USEFUL, PLEASE CONSIDER MAKING A DONATION    ;;;
;;;  OF $15.00 TO THE PERSON LISTED ABOVE.                                 ;;;
;;;************************************************************************;;;
;;;                                                                        ;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;                                                                        ;;;
;;;                          SMASH.LSP                                     ;;;
;;;                                                                        ;;;
;;;  Written by:  Jeffrey S. Pilcher - Thu  06-04-1992  07:16:50           ;;;
;;;                                                                        ;;;
;;;  SMASH all point values to have a Z value of ZERO (0).                 ;;;
;;;                                                                        ;;;
;;;  SMASH.LSP is a program that will change all POINT values              ;;;
;;;  that are associated with any drawing entity to have a 'Z'             ;;;
;;;  value of zero (0).  This is accomplished by replacing all             ;;;
;;;  'Z' coordinates with a value of zero (0), but maintaining             ;;;
;;;  the existing 'X' and 'Y' values.                                      ;;;
;;;                                                                        ;;;
;;;  In an effort to end up with a drawing that is truely 2D, I            ;;;
;;;  chose to then set the 'EXTRUSION DIRECTION' to point into             ;;;
;;;  the 'Z' axis of the world coordinate system.  This is done            ;;;
;;;  by changing ASSOC code 210 to a value of 0,0,1.                       ;;;
;;;                                                                        ;;;
;;;  My reason for writing this program was to fix drawings that           ;;;
;;;  might have a stray 3D entity.  Perhaps several entities ended up      ;;;
;;;  at a different elevation, or one endpoint of a line somehow           ;;;
;;;  was placed at a different Z coordinate than the other endpoint.       ;;;
;;;                                                                        ;;;
;;;  ******************************************************************    ;;;
;;;  ****  My goal was  NOT  to write a program that would convert ****    ;;;
;;;  ****  all 3D drawings to 2D drawings.  It has not been tested ****    ;;;
;;;  ****  to see if it is capable of converting all 3D to 2D.     ****    ;;;
;;;  ******************************************************************    ;;;
;;;                                                                        ;;;
;;;  By setting the 'EXTRUSION DIRECTION' back to what I consider          ;;;
;;;  a normal 2D value, you might experience an undesirable result.        ;;;
;;;  If there are entities drawn in a UCS that is not parallel to the      ;;;
;;;  World Coordinate System, they are changed so that they are in         ;;;
;;;  the WCS.  The result of this can be that circles and arcs that are    ;;;
;;;  drawn in a UCS, might have appeared to be ellipses in the WCS.        ;;;
;;;  These entities will be changed to be WCS circles, and the plan        ;;;
;;;  view of the WCS will appear different than before.                    ;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;                                                                        ;;;
;;;  VARIABLES                                                             ;;;
;;;                                                                        ;;;
;;;    local                                                               ;;;
;;;                                                                        ;;;
;;;      ent            ENAME,   AutoLISP database entity name             ;;;
;;;      entl           LIST,    AutoLISP entity data (points,layer,etc)   ;;;
;;;      cnt            INTEGER, number displayed as a counter             ;;;
;;;      typ            STRING,  entity type extracted from entl           ;;;
;;;                                                                        ;;;
;;;    global                                                              ;;;
;;;      there are no global variables in this program                     ;;;
;;;                                                                        ;;;
;;;  PASSED PARAMETERS                                                     ;;;
;;;    there are no parameters passed to this program                      ;;;
;;;                                                                        ;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;                                                                        ;;;
;;;  SUB-ROUTINES                                                          ;;;
;;;                                                                        ;;;
;;;    C:SMASH          Main program, loops thru all entities              ;;;
;;;    DO_POINTS        Process point values, convert to 2D points         ;;;
;;;    UPDATE           Update current entity, use newly created 2D point  ;;;
;;;                                                                        ;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;                                                                        ;;;
;;;  OTHER ROUTINES NECESSARY                                              ;;;
;;;    there are no other routines needed to run this program              ;;;
;;;                                                                        ;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(defun c:SMASHCMD ( / ent cnt entl typ)  ; define function and declare variables

  (setq ent (entnext)                 ; get first entity
        cnt 1                         ; set counter to starting value
  )                                   ; finished setting variables

  (while ent                          ; as long as there is an entity

    (setq entl (entget ent)           ; get LISP data for current entity
          typ (cdr (assoc 0 entl))    ; get current entity TYPE (LINE,ARC,etc)
    )                                 ; finished setting variables

    (prompt (strcat "\rChecking entity #" (rtos cnt 2 0))) ; print counter

    (redraw ent 3)                    ; highlight current entity

                                      ; depending on entity type, run
                                      ; do_points to change the listed points
    (cond ((= typ "LINE")(do_points '(10 11 210)))
          ((= typ "ARC")(do_points '(10 210)))
          ((= typ "POLYLINE")(do_points '(10 210)))
          ((= typ "VERTEX")(do_points '(10)))
          ((= typ "TEXT")(do_points '(10 11 210)))
          ((= typ "ATTRIBUTE")(do_points '(10 11 210)))
          ((= typ "CIRCLE")(do_points '(10 210)))
          ((= typ "INSERT")(do_points '(10 210)))
          ((= typ "SOLID")(do_points '(10 11 12 13)))
          ((= typ "POINT")(do_points '(10 210)))
    )

    (redraw ent 4)                    ; unhighlight current entity

    (setq ent (entnext ent)           ; get the next drawing entity
          cnt (1+ cnt)                ; increment counter
    )                                 ; finished setting variables

  )                                   ; end of the WHILE loop

  (princ)                             ; print nothing as program ends

)                                     ; end of the program

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;  DO_POINTS is called by C:SMASH from within the COND statement.  The   ;;;
;;;  assoc codes of the points to be modified are passed to DO_POINTS as   ;;;
;;;  a LIST.                                                               ;;;
;;;                      ie:  (do_points '(10 11 210))                     ;;;
;;;                                                                        ;;;
;;;  This line calls DO_POINTS and tells it to modify the indicated entity ;;;
;;;  information.                                                          ;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
(defun do_points (point_list / new)   ; define function and declare variables

  (foreach x point_list               ; for every item in POINT_LIST

    (update ent x                     ; call UPDATE and pass it ENAME
                                      ; ASSOC and NEW VALUE

      (if (/= x 210)                  ; if code is not 210 (extrusion dir)

        (list (cadr (assoc x entl))   ; create new point using existing X
              (caddr (assoc x entl))  ; use existing Y
              0.0                     ; use new Z of 0.0
        )                             ; finished building new point

        (list 0.0 0.0 1.0)            ; if code is 210 make it   0,0,1

      )                               ; end of IF
    )                                 ; end of call to UPDATE
  )                                   ; end of FOREACH loop
)                                     ; end of sub-routine

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;  UPDATE is called by DO_POINTS and actually does the work of           ;;;
;;;  redefining each entity as it is processed.  UPDATE is a generic       ;;;
;;;  routine that can be used to modify any aspect of any entity.          ;;;
;;;                                                                        ;;;
;;;  If you wanted to modify a LINE to be on a different LAYER, you could  ;;;
;;;  use UPDATE to acomplish this.                                         ;;;
;;;                                                                        ;;;
;;;                  ie:  (update ent 8 "BORDER")                          ;;;
;;;                                                                        ;;;
;;;  This example would change the entity whose ENAME was stored in ENT    ;;;
;;;  so that it would be placed on a layer called BORDER, if it existed.   ;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
(defun update (e dxfcode newvalue / elist) ; define function and declare vars
  (setq elist (entget e))                  ; get LISP database for entity

                                           ; do type check to make sure new
                                           ; data is valid for this accos code
  (if (= (type newvalue)(type (cdr (assoc dxfcode elist))))

                                           ; modify the entity database
    (entmod (subst (cons dxfcode newvalue)(assoc dxfcode elist) elist))
  )
  (entupd e)                               ; refresh entity on screen
)

(prompt "SMASHCMD zum Starten")
