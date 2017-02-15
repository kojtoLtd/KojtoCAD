;;;                                                                    ;
;;;  PEDIT3D.LSP                                                       ;
;;;                                                                    ;
;;;  Copyright 2000, 2001, 2004, 2006                                  ;
;;;  by Dr. H.-J. Schulz, Borntal 9, D-36457 Stadtlengsfeld            ;
;;;  eMail scj.schulz@t-online.de                                      ;
;;;  www.black-cad.de                                                  ; 
;;;  All Rights Reserved.                                              ;
;;;                                                                    ;
;;;  You are hereby granted permission to use, copy and modify this    ;
;;;  software without charge, provided you do so exclusively for       ;
;;;  your own use.                                                     ;
;;;                                                                    ;
;;;  Incorporation of any part of this software into other software,   ;
;;;  except when such incorporation is exclusively for your own use    ;
;;;  is prohibited.                                                    ;        
;;;                                                                    ;
;;;  Copying, modification and distribution of this software or any    ;
;;;  part thereof in any form except as expressly provided herein is   ;
;;;  prohibited without the consent of the author.                     ;
;;;                                                                    ;
;;;  THE AUTHOR PROVIDES THIS SOFTWARE "AS IS" AND WITH ALL FAULTS.    ;
;;;  THE AUTHOR SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF         ;
;;;  MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. THE AUTHOR DOES  ;
;;;  NOT WARRANT THAT THE OPERATION OF THE SOFTWARE WILL BE            ;
;;;  UNINTERRUPTED OR ERROR FREE.                                      ;
;;;                                                                    ;
;;;                                                                    ;
;;;--------------------------------------------------------------------;
;;;     Function: PEDIT3D                                              ;
;;;--------------------------------------------------------------------;
;;;  Description: This function will ask the user to select several    ;
;;;               linesegments - for instance lines, 2d- and           ;
;;;		  3d-circular or elliptical arcs, 2D- or 3D-polylines, ;
;;;		  LWPolylines or splines to join them into an unique   ; 
;;;               3d-polyline, consisting of  linear segments.         ; 
;;;               Depending on a fuzzy-distance gaps will be closed.   ;
;;;--------------------------------------------------------------------;
;-----------------------------------------------------------------------
(defun PARA()
   (setq spldiv 100)    ; spline is to divide in 100 linear segments
   (setq fuzz 0.001)	; fuzzy gap to close polylines
   (setq arcdiv 10)	; quarter arc (90ø) is to divide in 10 segments
) 
;------------------------------------------------------   
(defun xplo( / aa)
	(command "_explode" (ssname selection_set i))
	(setq aa (ssget "_P"))
	(setq ii 0)
	(repeat (sslength aa)
		(progn
			(ssadd (ssname aa ii) selection_set)
			(setq ii (+ 1 ii))
		)
	)
)
;------------------------------------------------------
(defun LITPX (sss / li pkt1 pkt2 plist)
	(setq pkt1 (cdr (assoc 10 sss)))
	(setq pkt2 (cdr (assoc 11 sss)))
	(setq plist (append plist (list pkt1)))
	(setq plist (append plist (list pkt2)))
	(setq pplist (append pplist (list plist)))
)
;------------------------------------------------------
(defun SPTPX (sss / pa pz ptset pkt ll plist ispl)
	(setq pa (cdr (assoc 10 (entget sss ))))
	(setq pz (cdr (assoc 10 (reverse (entget sss))))) 
	(command "_divide" sss spldiv)
	(setq ptset (ssget "_p"))
	(setq ll (sslength ptset))
	(setq plist (list pa))
	(setq ispl -1)  
	(setq ll (- ll 1))
	(while (< ispl ll)
		(progn
			(setq ispl (+ 1 ispl))
			(setq pkt (cdr (assoc 10 (entget (ssname ptset ispl)))))
			(setq plist (append (list pkt) plist))
		)
	)
	(command "_erase" ptset "")
	(setq plist (append (list pz) plist)) 
	(setq pplist (append pplist (list plist)))
)
;------------------------------------------------------
(defun ARTPX(sss / aufl ra pm aw ew sw ii pt plist)
			(command "_UCS" "_E" (ssname selection_set i))
			(setq ra        (cdr (assoc 40 sss)))
			(setq pm        (list 0 0))
			(setq aw        (cdr (assoc 50 sss)))
			(setq ew        (cdr (assoc 51 sss)))
			(if (< ew aw) 
				(setq ew (+ (* 2 pi) ew))
			)
			(setq aufl  arcdiv)
			(if (> (- ew aw) (* 0.5 pi)) (setq aufl (* 2 arcdiv)))
			(if (> (- ew aw) pi) (setq aufl (* 3 arcdiv)))
			(if (> (- ew aw) (* 1.5 pi)) (setq aufl (* 4 arcdiv)))
			(setq sw        (/  (- ew aw ) aufl))
			(setq aw 0)
			(setq sw        (abs sw))
			(setq ii 0)
			(while (<= ii aufl)
				(setq pt (polar pm (+ aw (* ii sw)) ra))
				(setq ptw (trans pt 1 0))
				(setq plist (append plist (list ptw)))
				(setq ii (+ ii 1))
			)
	(setq pplist (append pplist (list plist)))
)
;------------------------------------------------------
(defun ELTPX(sss / elli aufl pm p1 zr fak aw ew 
			pmx pmy pmz p1x p1y p1z zrx zry zrz 
			p1w zrw a b sw ii pt plist)
			(setq pm        (cdr  (assoc 10 sss)))
			(setq p1        (cdr  (assoc 11 sss)))
			(setq zr        (cdr (assoc 210 sss)))
			(setq fak       (cdr (assoc 40 sss)))
			(setq aw        (cdr (assoc 41 sss)))
			(setq ew        (cdr (assoc 42 sss)))

			(setq pmx       (car pm))
			(setq pmy       (cadr pm))
			(setq pmz       (caddr pm))       
			(setq p1x       (car p1))
			(setq p1y       (cadr p1))
			(setq p1z       (caddr p1))       
			(setq zrx       (car zr))
			(setq zry       (cadr zr))
			(setq zrz       (caddr zr))     
			(setq p1w (list (+ pmx p1x) (+ pmy p1y) (+ pmz p1z)))  
		(setq zrw (list (+ pmx zrx) (+ pmy zry) (+ pmz zrz)))  
			(setq a (distance (list 0 0 0) p1))
			(setq b (* fak a))
			(command "_UCS" "_3P"  pm p1w zrw)
			(command "_UCS" "_X" (- (* 0.5 pi)))
			(if (< ew aw) 
				(setq ew (+ (* 2 pi) ew))
			)
			(setq aufl arcdiv)
			(if (> (- ew aw) (* 0.5 pi)) (setq aufl (* 2 arcdiv)))
			(if (> (- ew aw) pi) (setq aufl (* 3 arcdiv)))
			(if (> (- ew aw) (* 1.5 pi)) (setq aufl (* 4 arcdiv)))
			(setq sw        (/  (- ew aw ) aufl))
			(setq ii 0)
			(while (<= ii aufl)
				(setq ptx  (* a (cos(+ aw (* ii sw)))))
				(setq pty  (* b (sin(+ aw (* ii sw)))))
				(setq pt (list ptx pty 0))
				(setq ptw (trans pt 1 0))
				(setq plist (append plist (list ptw)))
				(setq ii (+ ii 1))
			)
	(setq pplist (append pplist (list plist)))
)
;------------------------------------------------------
(defun Search_Append_Delete(uli / flag i po1 po2 pu pu1 pu2) 
(setq flag F)
(setq i -1)
(while (and (< i (length unord_list)) (= flag F))
	(progn
		(setq i (+ 1 i))
		(setq po1 (car ord_list) po2 (last ord_list))
		(setq pu (nth i unord_list))
		(setq pu1 (car pu) pu2 (last pu))
		(if (> fuzz (distance po2 pu1))
			(progn
	(setq ord_list (append ord_list (cdr pu)))
	(setq unord_list (apply 'append (subst nil (list pu) (mapcar 'list unord_list))))
	(setq flag T)
			) 
		)
		(if (and (> fuzz (distance po2 pu2)) (= flag F))
			(progn
	(setq ord_list (append ord_list (cdr (reverse pu))))
	(setq unord_list (apply 'append (subst nil (list pu) (mapcar 'list unord_list))))
	(setq flag T)
			)
		)
		(if (and (> fuzz (distance po1 pu1)) (= flag F))
			(progn
	(setq ord_list (append (reverse ord_list) (cdr pu)))
	(setq unord_list (apply 'append (subst nil (list pu) (mapcar 'list unord_list))))
	(setq flag T)
			)
		)
		(if (and (> fuzz (distance po1 pu2)) (= flag F))
			(progn
	(setq ord_list (append (reverse ord_list) (cdr (reverse pu))))
	(setq unord_list (apply 'append (subst nil (list pu) (mapcar 'list unord_list))))
	(setq flag T)
			)
		)
	)                                       
)                                              
unord_list
)
;------------------------------------------------------
;------------------------------------------------------
(defun C:PEDIT3DCMD(/ osmode_old aunits_old pplist selection_set typ i 
		unord_list ord_list pt fuzz arcdiv spldiv)
	
	(grtext -1 "PEDIT3DCMD")
	(command "_cmdecho" 0)
	(setq osmode_old (getvar "OSMODE"))
	(command "OSMODE" 0)
	(setq aunits_old (getvar "AUNITS"))
	(command "_AUNITS" 3)
	(para)
	(setq selection_set (ssget))  
	(command "_nomutt" "1")                        
;------------------------------------------------------
	(setq i 0)
	(while (< i (sslength selection_set))
		(progn
			(command "_UCS" "_W")
		(setq typ (cdr (assoc 0 (entget (ssname selection_set i)))))
		(if (= typ  "LWPOLYLINE") (xplo))
		(if (= typ  "POLYLINE")   (xplo))
			(setq i (+ 1 i))
		)
	)
;------------------------------------------------------
	(setq i 0)
	(while (< i (sslength selection_set))
		(progn
			(command "_UCS" "_W")
		(setq typ (cdr (assoc 0 (entget (ssname selection_set i)))))
		(if (= typ  "LINE")   (LITPX(entget(ssname selection_set i))))
		(if (= typ  "SPLINE") (SPTPX(ssname selection_set i)))
		(if (= typ  "ARC")    (ARTPX(entget(ssname selection_set i))))
		(if (= typ  "ELLIPSE")(ELTPX(entget(ssname selection_set i))))
			(setq i (+ 1 i))
		)
	)
;------------------------------------------------------
(command "_UCS" "_W")
(setq unord_list pplist)
(setq ord_list (car unord_list))
(setq unord_list (cdr unord_list))

	(while (< 0 (length unord_list))
		(Search_Append_Delete unord_list)
	)
(command "_3dpoly")
(foreach pt ord_list (command pt))
(command nil)
(grtext -1 "")
(command "_nomutt" "0")
(command "OSMODE" osmode_old)
(command "_AUNITS" aunits_old)
(command "_cmdecho" 1)
)